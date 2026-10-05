using System.Text;
using CloudinaryDotNet;
using CareFlowAI.API.Data;
using CareFlowAI.API.Services;
using CareFlowAI.Orchestrator;
using CareFlowAI.Orchestrator.Tools;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

DotNetEnv.Env.Load("../.env.local");
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});
builder.Services.AddEndpointsApiExplorer();

// Cloudinary configuration
var cloudinaryUrl = builder.Configuration["CLOUDINARY_URL"];

Console.WriteLine(
    $"[DEBUG] Initial CLOUDINARY_URL from config: " +
    $"{(string.IsNullOrWhiteSpace(cloudinaryUrl) ? "empty" : "configured")}");

if (!string.IsNullOrWhiteSpace(cloudinaryUrl))
{
    var cloudinary = new Cloudinary(cloudinaryUrl);
    cloudinary.Api.Secure = true;
    builder.Services.AddSingleton(cloudinary);
}

// Swagger with JWT Bearer support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CareFlowAI API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description =
            "JWT Authorization header using the Bearer scheme. " +
            "Example: \"Authorization: Bearer {token}\"",

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


// ============================================================
// Component B: Patient Context, Assessment & Planning
// ============================================================

builder.Services.AddScoped<
    IPatientContextTool,
    PatientContextTool>();

builder.Services.AddScoped<
    IClinicalAssessmentClient,
    GeminiAssessmentClient>();

builder.Services.AddScoped<CareFlowAI.Orchestrator.Agents.IDomainAnalysisAgent>(sp =>
{
    var tool = sp.GetRequiredService<IPatientHistoryTool>();
    return new CareFlowAI.Orchestrator.Agents.DomainAnalysisAgent(
        builder.Configuration["Gemini:ApiKey"] ?? string.Empty,
        tool,
        builder.Configuration["Gemini:Model"] ?? string.Empty
    );
});

builder.Services.AddScoped<
    PlanningAgentService>();

builder.Services.AddHostedService<
    CareFlowAI.AIOrchestrator.WorkflowManager>();


// ============================================================
// JWT & Security Services
// ============================================================

builder.Services.AddScoped<
    IJwtTokenService,
    JwtTokenService>();

var jwtSecret =
    builder.Configuration["Jwt:Secret"]
    ?? "CareFlowAI_Super_Secret_Key_For_Jwt_Signing_Must_Be_Long_Enough_2026!";

var jwtIssuer =
    builder.Configuration["Jwt:Issuer"]
    ?? "CareFlowAI";

var jwtAudience =
    builder.Configuration["Jwt:Audience"]
    ?? "CareFlowAI.Clients";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
        JwtBearerDefaults.AuthenticationScheme;

    options.DefaultChallengeScheme =
        JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters =
        new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,

            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSecret)),

            ClockSkew = TimeSpan.Zero
        };
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<IPatientHistoryTool, DbPatientHistoryTool>();

// ============================================================
// Database
// ============================================================

builder.Services.AddDbContext<ApplicationDbContext>(
    options =>
        options.UseNpgsql(
            builder.Configuration
                .GetConnectionString("DefaultConnection")));


// ============================================================
// Component C: Doctor Availability
// ============================================================

builder.Services.AddScoped<
    DoctorAvailabilityService>();


// ============================================================
// Component C: Appointment Scheduling
// ============================================================

builder.Services.AddScoped<
    AppointmentService>();


// ============================================================
// Component C: Provider Adapters
//
// These registrations are required by the Action Agent tools.
// The tools depend on interfaces instead of directly calling
// protected controller endpoints.
// ============================================================

builder.Services.AddScoped<
    CareFlowAI.Orchestrator.Abstractions.IAvailabilityProvider,
    AvailabilityProviderAdapter>();

builder.Services.AddScoped<
    CareFlowAI.Orchestrator.Abstractions.IAppointmentProvider,
    AppointmentProviderAdapter>();

builder.Services.AddScoped<
    CareFlowAI.Orchestrator.Abstractions.IAppointmentBookingProvider,
    AppointmentBookingProviderAdapter>();


// ============================================================
// Component C: Appointment Action Agent
//
// Allow-listed tools used by the Action Agent.
// ============================================================

builder.Services.AddScoped<
    CareFlowAI.Orchestrator.Tools.FindAvailableSlotsTool>();

builder.Services.AddScoped<
    CareFlowAI.Orchestrator.Tools.CheckBookingConflictTool>();

builder.Services.AddScoped<
    CareFlowAI.Orchestrator.Tools.CreateTentativeBookingTool>();

builder.Services.AddScoped<
    CareFlowAI.Orchestrator.Agents.AppointmentActionAgent>();

builder.Services.AddScoped<
    AppointmentWorkflowRunner>();


// ============================================================
// Component D: Third-Party Notifications & Pharmacy AI
// ============================================================

builder.Services.AddHttpClient<
    INotificationService,
    NotificationService>();

builder.Services.AddScoped<
    PharmacyAiService>();

builder.Services.AddScoped<ISafetyAgent>(
    sp =>
        sp.GetRequiredService<PharmacyAiService>());


// ============================================================
// CORS
// ============================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowReactApp",
        policy =>
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader());
});


var app = builder.Build();


// ============================================================
// HTTP Request Pipeline
// ============================================================

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


// Required for WebApplicationFactory integration tests.
public partial class Program { }