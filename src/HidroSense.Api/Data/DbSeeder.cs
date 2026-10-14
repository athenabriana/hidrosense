using HidroSense.Api.Models;
using HidroSense.Api.Security;
using Microsoft.EntityFrameworkCore;

namespace HidroSense.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, SeedSettings settings, CancellationToken ct = default)
    {
        if (await db.Users.AnyAsync(ct)) return;

        var seedTime = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        db.Users.AddRange(
            new AppUser { Username = "admin", Role = "Admin", PasswordHash = PasswordHasher.Hash(settings.AdminPassword), CreatedAt = seedTime },
            new AppUser { Username = "operator", Role = "Operator", PasswordHash = PasswordHasher.Hash(settings.OperatorPassword), CreatedAt = seedTime }
        );

        var units = new List<ConsumerUnit>
        {
            new() { Code = "RES-001", Name = "Residencial Jardim das Águas", Type = ConsumerUnitType.Residential, Region = "Zona Norte", IdleFlowThresholdLpm = 0.5, MonthlyBudgetLiters = 15000, Active = true, CreatedAt = seedTime },
            new() { Code = "COM-014", Name = "Shopping Vale Verde", Type = ConsumerUnitType.Commercial, Region = "Centro", IdleFlowThresholdLpm = 2.0, MonthlyBudgetLiters = 250000, Active = true, CreatedAt = seedTime },
            new() { Code = "IND-007", Name = "Fábrica Têxtil Aurora", Type = ConsumerUnitType.Industrial, Region = "Distrito Industrial", IdleFlowThresholdLpm = 5.0, MonthlyBudgetLiters = 800000, Active = true, CreatedAt = seedTime },
            new() { Code = "PUB-003", Name = "Hospital Municipal", Type = ConsumerUnitType.Public, Region = "Centro", IdleFlowThresholdLpm = 3.0, MonthlyBudgetLiters = 400000, Active = true, CreatedAt = seedTime }
        };
        db.ConsumerUnits.AddRange(units);
        await db.SaveChangesAsync(ct);

        var res = units[0];
        var com = units[1];

        db.ConsumptionReadings.AddRange(
            new ConsumptionReading { ConsumerUnitId = res.Id, ReadingTimeUtc = seedTime.AddHours(1), VolumeLiters = 120, FlowRateLpm = 0.3, CreatedAt = seedTime },
            new ConsumptionReading { ConsumerUnitId = res.Id, ReadingTimeUtc = seedTime.AddHours(2), VolumeLiters = 90, FlowRateLpm = 0.4, CreatedAt = seedTime },
            new ConsumptionReading { ConsumerUnitId = com.Id, ReadingTimeUtc = seedTime.AddHours(1), VolumeLiters = 3200, FlowRateLpm = 1.5, CreatedAt = seedTime }
        );

        db.LeakAlerts.Add(new LeakAlert
        {
            ConsumerUnitId = com.Id,
            DetectedAtUtc = seedTime.AddHours(3),
            Severity = LeakSeverity.High,
            Status = LeakAlertStatus.Open,
            TriggeringFlowRateLpm = 7.5,
            EstimatedWasteLiters = 330,
            Message = "Flow 7.5 L/min is 3.8x the idle baseline (2.0 L/min) for unit COM-014."
        });

        await db.SaveChangesAsync(ct);
    }
}
