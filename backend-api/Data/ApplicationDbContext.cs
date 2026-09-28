using Microsoft.EntityFrameworkCore;
using CareFlowAI.API.Models;

namespace CareFlowAI.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // ── Component A: Admissions (Sanuthmi) ───────────────────────────────
        public DbSet<PatientProfile> PatientProfiles { get; set; }
        public DbSet<Ward> Wards { get; set; }
        public DbSet<Admission> Admissions { get; set; }
        // ── Component B: Triage & AI (Sujana) ───────────────────────────────
        public DbSet<TriageRecord> TriageRecords { get; set; }
        public DbSet<AgentWorkflowState> AgentWorkflows { get; set; }
        public DbSet<TriageReviewHistory> TriageReviewHistories { get; set; }

        // ── Component C: Appointments & Resource Scheduling (Sandathi) ───────
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<DoctorAvailability> DoctorAvailabilities { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        // ── Component D: Pharmacy (Amodhya) ──────────────────────────────────
        public DbSet<Medicine> Medicines { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<PrescriptionItem> PrescriptionItems { get; set; }

        // ── Users for Web Admin Auth ─────────────────────────────────────────
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── TriageRecord ──────────────────────────────────────────────────
            modelBuilder.Entity<TriageRecord>(entity =>
            {
                entity.HasKey(t => t.Id);

                // FK → PatientProfiles (Component A's table).
                // We use WithMany() with no argument because PatientProfile's
                // Admissions collection already belongs to the Admission entity.
                entity.HasOne(t => t.Patient)
                      .WithMany()
                      .HasForeignKey(t => t.PatientId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Triage status constraint values
                entity.Property(t => t.TriageStatus)
                      .HasDefaultValue("Pending");

                entity.Property(t => t.SeverityLevel)
                      .HasDefaultValue("Medium");

                entity.Property(t => t.CreatedAt)
                      .HasDefaultValueSql("now()");

                entity.Property(t => t.UpdatedAt)
                      .IsConcurrencyToken()
                      .HasDefaultValueSql("now()");
            });

            // ── AgentWorkflowState ────────────────────────────────────────────
            modelBuilder.Entity<AgentWorkflowState>(entity =>
            {
                entity.HasKey(a => a.Id);

                // FK → TriageRecords (one triage → many agent workflow rows)
                entity.HasOne(a => a.TriageRecord)
                      .WithMany(t => t.AgentWorkflows)
                      .HasForeignKey(a => a.TriageRecordId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Property(a => a.AgentStatus)
                      .HasDefaultValue("Idle");

                entity.Property(a => a.ApprovalStatus)
                      .HasDefaultValue("Pending");

                entity.Property(a => a.CreatedAt)
                      .HasDefaultValueSql("now()");

                entity.Property(a => a.UpdatedAt)
                      .HasDefaultValueSql("now()");
            });

            // ── TriageReviewHistory ───────────────────────────────────────────
            modelBuilder.Entity<TriageReviewHistory>(entity =>
            {
                entity.HasKey(a => a.Id);

                entity.HasOne(a => a.TriageRecord)
                      .WithMany(t => t.ReviewHistories)
                      .HasForeignKey(a => a.TriageRecordId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.AgentWorkflowState)
                      .WithMany()
                      .HasForeignKey(a => a.AgentWorkflowStateId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.Property(a => a.CreatedAt)
                      .HasDefaultValueSql("now()");
            });

            // ── Component C: Doctor Availability ─────────────────────────────
            modelBuilder.Entity<DoctorAvailability>(entity =>
            {
                entity.HasKey(a => a.Id);

                entity.HasOne(a => a.Doctor)
                      .WithMany(d => d.Availabilities)
                      .HasForeignKey(a => a.DoctorId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ── Component C: Appointments ────────────────────────────────────
            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasKey(a => a.Id);

                entity.HasOne(a => a.Doctor)
                      .WithMany(d => d.Appointments)
                      .HasForeignKey(a => a.DoctorId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Patient)
                      .WithMany()
                      .HasForeignKey(a => a.PatientId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(a => a.Status)
                      .HasDefaultValue("Tentative");

                entity.Property(a => a.CreatedAt)
                      .HasDefaultValueSql("now()");
            });
            // ── Medicine ──────────────────────────────────────────────────────
            modelBuilder.Entity<Medicine>(entity =>
            {
                entity.HasKey(m => m.Id);

                entity.Property(m => m.IsActive)
                      .HasDefaultValue(true);

                entity.Property(m => m.StockQuantity)
                      .HasDefaultValue(0);

                entity.Property(m => m.ReorderLevel)
                      .HasDefaultValue(10);

                entity.Property(m => m.UnitPrice)
                      .HasColumnType("numeric(10,2)");

                entity.Property(m => m.CreatedAt)
                      .HasDefaultValueSql("now()");

                entity.Property(m => m.UpdatedAt)
                      .IsConcurrencyToken()
                      .HasDefaultValueSql("now()");
            });

            // ── Prescription ──────────────────────────────────────────────────
            modelBuilder.Entity<Prescription>(entity =>
            {
                entity.HasKey(p => p.Id);

                // FK → PatientProfiles (Component A)
                entity.HasOne(p => p.Patient)
                      .WithMany()
                      .HasForeignKey(p => p.PatientId)
                      .OnDelete(DeleteBehavior.Restrict);

                // FK → TriageRecords (Component B)
                entity.HasOne(p => p.TriageRecord)
                      .WithMany()
                      .HasForeignKey(p => p.TriageRecordId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(p => p.Status)
                      .HasDefaultValue("Draft");

                entity.Property(p => p.NotificationSent)
                      .HasDefaultValue(false);

                entity.Property(p => p.CreatedAt)
                      .HasDefaultValueSql("now()");

                entity.Property(p => p.UpdatedAt)
                      .IsConcurrencyToken()
                      .HasDefaultValueSql("now()");
            });

            // ── PrescriptionItem ──────────────────────────────────────────────
            modelBuilder.Entity<PrescriptionItem>(entity =>
            {
                entity.HasKey(pi => pi.Id);

                // FK → Prescriptions
                entity.HasOne(pi => pi.Prescription)
                      .WithMany(p => p.Items)
                      .HasForeignKey(pi => pi.PrescriptionId)
                      .OnDelete(DeleteBehavior.Cascade);

                // FK → Medicines
                entity.HasOne(pi => pi.Medicine)
                      .WithMany(m => m.PrescriptionItems)
                      .HasForeignKey(pi => pi.MedicineId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Seed Doctor
            var defaultDoctorId = Guid.Parse("22222222-2222-2222-2222-222222222220");
            modelBuilder.Entity<Doctor>().HasData(
                new Doctor { Id = defaultDoctorId, FullName = "Dr. Robert Smith", Specialization = "General Practitioner", Email = "doctor@careflow.ai", IsActive = true }
            );

            // Seed Users
            modelBuilder.Entity<User>().HasData(
                new User { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Username = "admin", Email = "admin@careflow.ai", Password = "password", Role = "Admin" },
                new User { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Username = "doctor", Email = "doctor@careflow.ai", Password = "password", Role = "Doctor", DoctorId = defaultDoctorId },
                new User { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Username = "staff", Email = "staff@careflow.ai", Password = "password", Role = "Staff" }
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
