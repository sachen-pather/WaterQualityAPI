using Microsoft.EntityFrameworkCore;
using WaterQualityAPI.Models;

namespace WaterQualityAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<WaterQualityReading> WaterQualityReadings { get; set; }
        public DbSet<CommunityPost> CommunityPosts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<WaterQualityReading>(entity =>
            {
                entity.ToTable("WaterQualityReadings");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.BeachCode).IsRequired();
                entity.Property(e => e.SamplingDate).IsRequired();
                entity.Property(e => e.SamplingFrequency).IsRequired();
            });

            modelBuilder.Entity<CommunityPost>(entity =>
            {
                entity.ToTable("CommunityPosts");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.BeachCode).IsRequired();
                entity.Property(e => e.Content).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.Status).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
            });

            // Modify CommunityPost relationship to reference BeachCode directly
            modelBuilder.Entity<CommunityPost>()
                .HasIndex(p => p.BeachCode); // Add index on BeachCode for better query performance
        }
    }
}