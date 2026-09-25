using CareFlowAI.API.Data;
using CareFlowAI.API.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Component D: Pharmacy AI Validation/Safety Agent
builder.Services.AddSingleton<PharmacyAiService>();

// Component D: Third-Party SMS & Email Notification Service
builder.Services.AddScoped<INotificationService, NotificationService>();

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

app.UseAuthorization();
app.MapControllers();

app.Run();