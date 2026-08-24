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
    }
}