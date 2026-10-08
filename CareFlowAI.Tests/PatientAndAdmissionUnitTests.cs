using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CareFlowAI.API.Controllers;
using CareFlowAI.API.Data;
using CareFlowAI.API.Models;
using System;
using System.Threading.Tasks;

public class PatientAndAdmissionUnitTests
{
    private ApplicationDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public async Task CreatePatient_WithValidData_ReturnsCreatedResult()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var controller = new PatientProfilesController(context);
        
        // Using the direct model instead of a DTO
        var newPatient = new PatientProfile
        {
            Id = Guid.NewGuid(),
            FullName = "Automated Test User",
            DateOfBirth = new DateOnly(1995, 6, 15), 
            BloodGroup = "O+",
            MedicalHistorySummary = "Routine checkup",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        // Act
        var result = await controller.RegisterPatient(newPatient);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var returnValue = Assert.IsType<PatientProfile>(createdResult.Value);
        Assert.Equal("Automated Test User", returnValue.FullName);
        Assert.Equal("O+", returnValue.BloodGroup);
    }

    [Fact]
    public async Task CreatePatient_WithInvalidBloodGroup_ReturnsBadRequest()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var controller = new PatientProfilesController(context);
        
        var invalidPatient = new PatientProfile
        {
            Id = Guid.NewGuid(),
            FullName = "Bad Blood Group User",
            DateOfBirth = new DateOnly(1990, 1, 1),
            BloodGroup = "Z+", // Invalid blood group
            MedicalHistorySummary = "None",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        // Act
        var result = await controller.RegisterPatient(invalidPatient);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task SearchPatients_ReturnsMatchingResults()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        context.PatientProfiles.Add(new PatientProfile
        {
            Id = Guid.NewGuid(),
            FullName = "Jane Doe",
            BloodGroup = "A-",
            DateOfBirth = new DateOnly(1992, 3, 10),
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });
        await context.SaveChangesAsync();

        var controller = new PatientProfilesController(context);

        // Act
        var result = await controller.SearchPatients("Jane");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var patients = Assert.IsAssignableFrom<System.Collections.Generic.IEnumerable<PatientProfile>>(okResult.Value);
    }
}

