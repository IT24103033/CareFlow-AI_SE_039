using Microsoft.EntityFrameworkCore;
using CareFlowAI.API.Models;

namespace CareFlowAI.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // ── Component A: Admissions (Sanuthmi) ───────────────────────────────
        public DbSet<PatientProfile> PatientProfiles { get; set; }
        public DbSet<Ward> Wards { get; set; }
        public DbSet<Admission> Admissions { get; set; }

        // ── Component B: Triage & AI (Sujana) ───────────────────────────────
        public DbSet<TriageRecord> TriageRecords { get; set; }
        public DbSet<AgentWorkflowState> AgentWorkflows { get; set; }

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
        }
    }
}