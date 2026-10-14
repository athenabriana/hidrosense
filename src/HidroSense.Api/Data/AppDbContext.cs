using HidroSense.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace HidroSense.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ConsumerUnit> ConsumerUnits => Set<ConsumerUnit>();
    public DbSet<ConsumptionReading> ConsumptionReadings => Set<ConsumptionReading>();
    public DbSet<LeakAlert> LeakAlerts => Set<LeakAlert>();
    public DbSet<AppUser> Users => Set<AppUser>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<ConsumerUnit>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Code).IsUnique();
            e.HasIndex(x => x.Region);
            e.Property(x => x.Code).IsRequired();
            e.Property(x => x.Name).IsRequired();
        });

        b.Entity<ConsumptionReading>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.ConsumerUnit)
                .WithMany(u => u.Readings)
                .HasForeignKey(x => x.ConsumerUnitId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => new { x.ConsumerUnitId, x.ReadingTimeUtc });
            e.HasIndex(x => x.ReadingTimeUtc);
        });

        b.Entity<LeakAlert>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.ConsumerUnit)
                .WithMany(u => u.LeakAlerts)
                .HasForeignKey(x => x.ConsumerUnitId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.Status);
            e.HasIndex(x => new { x.Status, x.Severity });
        });

        b.Entity<AppUser>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Username).IsUnique();
        });
    }
}
