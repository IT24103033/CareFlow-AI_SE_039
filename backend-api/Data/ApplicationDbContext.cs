using Microsoft.EntityFrameworkCore;
using CareFlowAI.API.Models;

namespace CareFlowAI.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<PatientProfile> PatientProfiles { get; set; }
        public DbSet<Ward> Wards { get; set; }
        public DbSet<Admission> Admissions { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed Users
            modelBuilder.Entity<User>().HasData(
                new User { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Username = "admin", Password = "password", Role = "Admin" },
                new User { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Username = "doctor", Password = "password", Role = "Doctor" },
                new User { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Username = "staff", Password = "password", Role = "Staff" }
            );

            // Seed Wards
            var generalId = Guid.Parse("44444444-4444-4444-4444-444444444441");
            var icuId = Guid.Parse("44444444-4444-4444-4444-444444444442");
            var matId = Guid.Parse("44444444-4444-4444-4444-444444444443");
            var pedId = Guid.Parse("44444444-4444-4444-4444-444444444444");
            
            modelBuilder.Entity<Ward>().HasData(
                new Ward { Id = generalId, WardNumber = "G-01", WardType = "General", Capacity = 50, OccupiedBeds = 45 },
                new Ward { Id = icuId, WardNumber = "I-01", WardType = "ICU", Capacity = 15, OccupiedBeds = 12 },
                new Ward { Id = matId, WardNumber = "M-01", WardType = "Maternity", Capacity = 30, OccupiedBeds = 20 },
                new Ward { Id = pedId, WardNumber = "P-01", WardType = "Pediatrics", Capacity = 25, OccupiedBeds = 10 }
            );

            // Seed Patients
            modelBuilder.Entity<PatientProfile>().HasData(
                new PatientProfile { Id = Guid.Parse("55555555-5555-5555-5555-555555555551"), FullName = "Sarah Jenkins", DateOfBirth = new DateOnly(1985, 3, 12), BloodGroup = "O+", MedicalHistorySummary = "No known allergies. Previous appendectomy." },
                new PatientProfile { Id = Guid.Parse("55555555-5555-5555-5555-555555555552"), FullName = "Marcus Thorne", DateOfBirth = new DateOnly(1972, 11, 5), BloodGroup = "A-", MedicalHistorySummary = "Type 2 Diabetes, Hypertension." },
                new PatientProfile { Id = Guid.Parse("55555555-5555-5555-5555-555555555553"), FullName = "Emily Chen", DateOfBirth = new DateOnly(1990, 7, 22), BloodGroup = "B+", MedicalHistorySummary = "Asthma, treated with inhalers." },
                new PatientProfile { Id = Guid.Parse("55555555-5555-5555-5555-555555555554"), FullName = "David Alaba", DateOfBirth = new DateOnly(1950, 1, 30), BloodGroup = "O-", MedicalHistorySummary = "Coronary artery disease, pacemaker fitted 2018." },
                new PatientProfile { Id = Guid.Parse("55555555-5555-5555-5555-555555555555"), FullName = "Fiona Gallagher", DateOfBirth = new DateOnly(2005, 9, 14), BloodGroup = "AB+", MedicalHistorySummary = "None." }
            );
        }
    }
}