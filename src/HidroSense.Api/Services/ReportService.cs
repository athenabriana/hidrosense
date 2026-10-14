using HidroSense.Api.Models;
using HidroSense.Api.Repositories;
using HidroSense.Api.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace HidroSense.Api.Services;

public interface IReportService
{
    Task<WaterImpactReportViewModel> BuildWaterImpactAsync(WaterImpactReportQuery query, CancellationToken ct = default);
}

public class ReportService : IReportService
{
    private readonly IConsumerUnitRepository _units;
    private readonly IConsumptionRepository _readings;
    private readonly ILeakAlertRepository _alerts;

    public ReportService(
        IConsumerUnitRepository units,
        IConsumptionRepository readings,
        ILeakAlertRepository alerts)
    {
        _units = units;
        _readings = readings;
        _alerts = alerts;
    }

    public async Task<WaterImpactReportViewModel> BuildWaterImpactAsync(WaterImpactReportQuery query, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var from = query.FromUtc ?? now.AddDays(-30);
        var to = query.ToUtc ?? now;

        var unitsByRegion = await _units.Query()
            .GroupBy(u => u.Region)
            .Select(g => new
            {
                Region = g.Key,
                Units = g.Count(),
                Budget = g.Sum(u => u.MonthlyBudgetLiters)
            })
            .ToListAsync(ct);

        var consumedByRegion = await _readings.Query()
            .Where(r => r.ReadingTimeUtc >= from && r.ReadingTimeUtc <= to)
            .GroupBy(r => r.ConsumerUnit!.Region)
            .Select(g => new { Region = g.Key, Consumed = g.Sum(r => r.VolumeLiters) })
            .ToListAsync(ct);

        var wasteByRegion = await _alerts.Query()
            .Where(a => a.Status == LeakAlertStatus.Open)
            .GroupBy(a => a.ConsumerUnit!.Region)
            .Select(g => new { Region = g.Key, OpenAlerts = g.Count(), Waste = g.Sum(a => a.EstimatedWasteLiters) })
            .ToListAsync(ct);

        var consumedMap = consumedByRegion.ToDictionary(x => x.Region, x => x.Consumed);
        var wasteMap = wasteByRegion.ToDictionary(x => x.Region, x => x);

        var regions = unitsByRegion
            .Select(u =>
            {
                var consumed = consumedMap.GetValueOrDefault(u.Region, 0d);
                var waste = wasteMap.GetValueOrDefault(u.Region);
                var openAlerts = waste?.OpenAlerts ?? 0;
                var wasteLiters = waste?.Waste ?? 0d;
                var usagePct = u.Budget > 0 ? Math.Round(consumed / u.Budget * 100, 1) : 0;

                return new RegionImpactViewModel
                {
                    Region = u.Region,
                    ConsumerUnits = u.Units,
                    TotalConsumedLiters = Math.Round(consumed, 2),
                    TotalBudgetLiters = Math.Round(u.Budget, 2),
                    BudgetUsagePercent = usagePct,
                    OpenLeakAlerts = openAlerts,
                    EstimatedWasteLiters = Math.Round(wasteLiters, 2),
                    MitigationPlan = BuildMitigationPlan(usagePct, openAlerts, wasteLiters)
                };
            })
            .OrderByDescending(r => r.EstimatedWasteLiters)
            .ToList();

        var totalConsumed = regions.Sum(r => r.TotalConsumedLiters);
        var totalWaste = regions.Sum(r => r.EstimatedWasteLiters);
        var overallWastePct = totalConsumed > 0 ? Math.Round(totalWaste / totalConsumed * 100, 1) : 0;

        return new WaterImpactReportViewModel
        {
            FromUtc = from,
            ToUtc = to,
            TotalConsumedLiters = Math.Round(totalConsumed, 2),
            TotalEstimatedWasteLiters = Math.Round(totalWaste, 2),
            OverallWastePercent = overallWastePct,
            Regions = regions
        };
    }

    private static string BuildMitigationPlan(double usagePct, int openAlerts, double wasteLiters)
    {
        var actions = new List<string>();
        if (openAlerts > 0)
            actions.Add($"Dispatch field teams to {openAlerts} open leak(s) (~{wasteLiters:F0} L/h at risk).");
        if (usagePct >= 90)
            actions.Add("Usage near contracted budget: trigger demand-reduction campaign and pressure management.");
        else if (usagePct >= 75)
            actions.Add("Usage elevated: schedule audit of high-consumption units.");
        if (actions.Count == 0)
            actions.Add("Consumption within healthy range: maintain monitoring cadence.");
        return string.Join(" ", actions);
    }
}
