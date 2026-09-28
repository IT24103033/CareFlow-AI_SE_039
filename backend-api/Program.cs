using System.Text;
using CareFlowAI.API.Data;
using CareFlowAI.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger with JWT Bearer support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CareFlowAI API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Component B: controlled context access, model adapter and planner.
builder.Services.AddScoped<IPatientContextTool, PatientContextTool>();
builder.Services.AddScoped<IClinicalAssessmentClient, GeminiAssessmentClient>();
builder.Services.AddScoped<PlanningAgentService>();

// JWT & Security Services
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// Authentication & JWT Bearer configuration
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "CareFlowAI_Super_Secret_Key_For_Jwt_Signing_Must_Be_Long_Enough_2026!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "CareFlowAI";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "CareFlowAI.Clients";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Component D: Third-Party SMS & Email Notification Service
builder.Services.AddHttpClient<INotificationService, NotificationService>();
builder.Services.AddScoped<PharmacyAiService>();
builder.Services.AddScoped<ISafetyAgent>(sp => sp.GetRequiredService<PharmacyAiService>());

// Component C: Doctor Availability Service
builder.Services.AddScoped<DoctorAvailabilityService>();

// Create the CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ACTIVATE the CORS policy
app.UseCors("AllowReactApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Required for WebApplicationFactory in integration tests
public partial class Program { }