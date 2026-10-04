using System.Text;
using CareFlowAI.API.Data;
using CareFlowAI.API.Services;
using CareFlowAI.Orchestrator.Tools;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "CareFlow AI API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IPatientHistoryTool, DbPatientHistoryTool>();
builder.Services.AddSingleton<AuthTokenService>();

var jwt = builder.Configuration.GetSection("Jwt");
var secret = jwt["Secret"] ?? "CareFlowAI_Dev_Only_Signing_Key_Change_Me_32";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"] ?? "CareFlowAI",
            ValidAudience = jwt["Audience"] ?? "CareFlowAI.Clients",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await EnsureDemoUsersAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowReactApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

static async Task EnsureDemoUsersAsync(ApplicationDbContext db)
{
    var demoUsers = new[]
    {
        new { Username = "admin", Password = "password", Role = "Admin" },
        new { Username = "doctor", Password = "password", Role = "Doctor" },
        new { Username = "staff", Password = "password", Role = "Staff" }
    };

    foreach (var demo in demoUsers)
    {
        var existing = await db.Users.FirstOrDefaultAsync(u =>
            EF.Functions.ILike(u.Username, demo.Username));

        if (existing == null)
        {
            db.Users.Add(new CareFlowAI.API.Models.User
            {
                Username = demo.Username,
                Password = demo.Password,
                Role = demo.Role
            });
        }
        else
        {
            existing.Password = demo.Password;
            existing.Role = demo.Role;
        }
    }

    await db.SaveChangesAsync();
}

