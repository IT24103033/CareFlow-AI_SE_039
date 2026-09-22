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

        // ── Component C: Appointments & Resource Scheduling (Sandathi) ───────
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<DoctorAvailability> DoctorAvailabilities { get; set; }
        public DbSet<Appointment> Appointments { get; set; }

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
        }
    }
}
