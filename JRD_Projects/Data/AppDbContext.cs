using Microsoft.EntityFrameworkCore;
using JRD_Projects.Models;

namespace JRD_Projects.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<VisitorLog> VisitorLog { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VisitorLog>().ToTable("VisitorLog");

            modelBuilder.Entity<VisitorLog>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Timestamp).HasColumnName("timestamp");
                //entity.Property(e => e.Ip).HasColumnName("ip");
                //entity.Property(e => e.UserAgent).HasColumnName("user_agent");
                //entity.Property(e => e.IsOwner).HasColumnName("is_owner");
                entity.Property(e => e.Location).HasColumnName("location");
            });
        }
    }
}
