using Microsoft.EntityFrameworkCore;
using WaterQualityAPI.Models;

namespace WaterQualityAPI.Data
{
    public class SupabaseContext : DbContext
    {
        public SupabaseContext(DbContextOptions<SupabaseContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<Beach> Beaches { get; set; }
        public DbSet<WaterQualityReading> WaterQualityReadings { get; set; }
        public DbSet<CommunityPost> CommunityPosts { get; set; }
        public DbSet<CommunityDiscussion> CommunityDiscussions { get; set; }
        public DbSet<CommunityComment> CommunityComments { get; set; }
        public DbSet<CommunityDiscussionPost> CommunityDiscussionPosts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Beach configuration
            modelBuilder.Entity<Beach>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Code).IsUnique();
                entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Location).IsRequired().HasMaxLength(500);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // WaterQualityReading configuration
            modelBuilder.Entity<WaterQualityReading>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.BeachCode);
                entity.Property(e => e.BeachCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.SamplingFrequency).IsRequired().HasMaxLength(50);

                // Foreign key relationship with Beach
                entity.HasOne<Beach>()
                    .WithMany(b => b.WaterQualityReadings)
                    .HasForeignKey(r => r.BeachCode)
                    .HasPrincipalKey(b => b.Code)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // CommunityPost configuration
            modelBuilder.Entity<CommunityPost>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.BeachCode);
                entity.Property(e => e.BeachCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Content).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.Status).HasDefaultValue("pending").HasMaxLength(20);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Foreign key relationship with Beach
                entity.HasOne<Beach>()
                    .WithMany()
                    .HasForeignKey(p => p.BeachCode)
                    .HasPrincipalKey(b => b.Code)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Other entity configurations...
            modelBuilder.Entity<CommunityDiscussion>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.Category).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            modelBuilder.Entity<CommunityComment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.DiscussionId);
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(c => c.Discussion)
                    .WithMany()
                    .HasForeignKey(c => c.DiscussionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CommunityDiscussionPost>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.Category).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
        }
    }
}