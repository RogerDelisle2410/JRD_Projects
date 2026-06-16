using Microsoft.EntityFrameworkCore;
using JRD_Projects.Models;

namespace JRD_Projects.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<VisitorCount> VisitorCount { get; set; }
        public DbSet<VisitorLog> VisitorLog { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Map VisitorCount to existing BCATP table
            modelBuilder.Entity<VisitorCount>().ToTable("VisitorCount");

            modelBuilder.Entity<VisitorCount>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.count).HasColumnName("count");
                entity.Property(e => e.today_visits).HasColumnName("today_visits");
                entity.Property(e => e.last_visit).HasColumnName("last_visit");
                entity.Property(e => e.last_ip).HasColumnName("last_ip");
                entity.Property(e => e.last_user_agent).HasColumnName("last_user_agent");
            });

            // Map VisitorLog to existing BCATP table
            modelBuilder.Entity<VisitorLog>().ToTable("VisitorLog");

            modelBuilder.Entity<VisitorLog>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Timestamp).HasColumnName("timestamp");
                entity.Property(e => e.Ip).HasColumnName("ip");
                entity.Property(e => e.UserAgent).HasColumnName("user_agent");
                entity.Property(e => e.IsOwner).HasColumnName("is_owner");
                entity.Property(e => e.Location).HasColumnName("location");
            });
        }
    }
}
