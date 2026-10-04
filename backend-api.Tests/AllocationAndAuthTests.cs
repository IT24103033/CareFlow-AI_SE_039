using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CareFlowAI.API.Data;
using CareFlowAI.API.Models;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CareFlowAI.API.Tests;

[Collection("postgres")]
public class AllocationAndAuthTests
{
    private readonly PostgresApiFixture _fixture;

    public AllocationAndAuthTests(PostgresApiFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Allocate_succeeds_when_ward_has_capacity()
    {
        await using var factory = new ApiFactory(_fixture.ConnectionString);
        factory.ResetDatabase();
        var client = factory.CreateClient();
        var token = await LoginAsync(client, "staff", "password");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var patientId = Guid.Parse("55555555-5555-5555-5555-555555555555");
        var wardId = Guid.Parse("44444444-4444-4444-4444-444444444444");

        var response = await client.PostAsJsonAsync("/api/admissions/allocate-ward", new
        {
            patientProfileId = patientId,
            wardId
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var ward = await db.Wards.FindAsync(wardId);
        Assert.Equal(11, ward!.OccupiedBeds);
        Assert.Equal(1, db.Admissions.Count(a => a.PatientProfileId == patientId && a.DischargedAt == null));
    }

    [Fact]
    public async Task Allocate_rejects_full_ward()
    {
        await using var factory = new ApiFactory(_fixture.ConnectionString);
        factory.ResetDatabase();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var ward = await db.Wards.FindAsync(Guid.Parse("44444444-4444-4444-4444-444444444444"));
            ward!.OccupiedBeds = ward.Capacity;
            await db.SaveChangesAsync();
        }

        var client = factory.CreateClient();
        var token = await LoginAsync(client, "staff", "password");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync("/api/admissions/allocate-ward", new
        {
            patientProfileId = Guid.Parse("55555555-5555-5555-5555-555555555555"),
            wardId = Guid.Parse("44444444-4444-4444-4444-444444444444")
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains("full", body.GetProperty("error").GetString(), StringComparison.OrdinalIgnoreCase);
        Assert.True(body.TryGetProperty("correlationId", out _));
    }

    [Fact]
    public async Task Concurrent_last_bed_allows_only_one_allocation()
    {
        await using var factory = new ApiFactory(_fixture.ConnectionString);
        factory.ResetDatabase();

        var wardId = Guid.NewGuid();
        var patientA = Guid.Parse("55555555-5555-5555-5555-555555555551");
        var patientB = Guid.Parse("55555555-5555-5555-5555-555555555552");

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Wards.Add(new Ward
            {
                Id = wardId,
                WardNumber = "LAST-1",
                WardType = "General",
                Capacity = 1,
                OccupiedBeds = 0
            });
            await db.SaveChangesAsync();
        }

        var clientA = factory.CreateClient();
        var clientB = factory.CreateClient();
        var token = await LoginAsync(clientA, "staff", "password");
        clientA.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        clientB.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var taskA = clientA.PostAsJsonAsync("/api/admissions/allocate-ward", new { patientProfileId = patientA, wardId });
        var taskB = clientB.PostAsJsonAsync("/api/admissions/allocate-ward", new { patientProfileId = patientB, wardId });
        await Task.WhenAll(taskA, taskB);

        var statuses = new[] { (await taskA).StatusCode, (await taskB).StatusCode };
        Assert.Contains(HttpStatusCode.OK, statuses);
        Assert.Equal(1, statuses.Count(s => s == HttpStatusCode.OK));
        Assert.DoesNotContain(HttpStatusCode.InternalServerError, statuses);

        using var verify = factory.Services.CreateScope();
        var db2 = verify.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var ward = await db2.Wards.FindAsync(wardId);
        Assert.Equal(1, ward!.OccupiedBeds);
        Assert.Equal(1, db2.Admissions.Count(a => a.WardId == wardId && a.DischargedAt == null));
    }

    [Fact]
    public async Task Allocate_without_token_is_unauthorized()
    {
        await using var factory = new ApiFactory(_fixture.ConnectionString);
        factory.ResetDatabase();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/admissions/allocate-ward", new
        {
            patientProfileId = Guid.NewGuid(),
            wardId = Guid.NewGuid()
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Staff_cannot_update_or_delete_patients()
    {
        await using var factory = new ApiFactory(_fixture.ConnectionString);
        factory.ResetDatabase();
        var client = factory.CreateClient();
        var token = await LoginAsync(client, "staff", "password");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var patientId = Guid.Parse("55555555-5555-5555-5555-555555555555");

        var put = await client.PutAsJsonAsync($"/api/PatientProfiles/{patientId}", new
        {
            fullName = "Fiona Gallagher",
            dateOfBirth = "2005-09-14",
            bloodGroup = "AB+",
            medicalHistorySummary = "Changed by staff"
        });
        var delete = await client.DeleteAsync($"/api/PatientProfiles/{patientId}");

        Assert.Equal(HttpStatusCode.Forbidden, put.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, delete.StatusCode);
    }

    [Fact]
    public async Task Duplicate_active_admission_is_rejected()
    {
        await using var factory = new ApiFactory(_fixture.ConnectionString);
        factory.ResetDatabase();
        var client = factory.CreateClient();
        var token = await LoginAsync(client, "staff", "password");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new
        {
            patientProfileId = Guid.Parse("55555555-5555-5555-5555-555555555553"),
            wardId = Guid.Parse("44444444-4444-4444-4444-444444444444")
        };

        var first = await client.PostAsJsonAsync("/api/admissions/allocate-ward", request);
        var second = await client.PostAsJsonAsync("/api/admissions/allocate-ward", request);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    private static async Task<string> LoginAsync(HttpClient client, string username, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new { username, password });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("token").GetString()!;
    }
}
