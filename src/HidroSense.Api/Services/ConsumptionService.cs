using HidroSense.Api.Middleware;
using HidroSense.Api.Models;
using HidroSense.Api.Repositories;
using HidroSense.Api.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace HidroSense.Api.Services;

public interface IConsumptionService
{
    Task<ConsumptionReadingResultViewModel> IngestAsync(CreateConsumptionReadingViewModel input, CancellationToken ct = default);
    Task<PagedResult<ConsumptionReadingViewModel>> ListAsync(ConsumptionReadingFilter filter, CancellationToken ct = default);
}

public class ConsumptionService : IConsumptionService
{
    private const double WasteWindowMinutes = 60.0;

    private readonly IConsumptionRepository _readings;
    private readonly IConsumerUnitRepository _units;
    private readonly ILeakAlertRepository _alerts;

    public ConsumptionService(
        IConsumptionRepository readings,
        IConsumerUnitRepository units,
        ILeakAlertRepository alerts)
    {
        _readings = readings;
        _units = units;
        _alerts = alerts;
    }

    public async Task<ConsumptionReadingResultViewModel> IngestAsync(CreateConsumptionReadingViewModel input, CancellationToken ct = default)
    {
        var unit = await _units.GetByIdAsync(input.ConsumerUnitId, ct)
            ?? throw new NotFoundException($"Consumer unit {input.ConsumerUnitId} was not found.");

        if (!unit.Active)
            throw new DomainException("Cannot ingest readings for an inactive consumer unit.");

        var now = DateTime.UtcNow;
        var reading = new ConsumptionReading
        {
            ConsumerUnitId = unit.Id,
            ReadingTimeUtc = input.ReadingTimeUtc ?? now,
            VolumeLiters = input.VolumeLiters,
            FlowRateLpm = input.FlowRateLpm,
            CreatedAt = now
        };

        await _readings.AddAsync(reading, ct);

        var alert = TryBuildLeakAlert(unit, reading, now);
        if (alert is not null)
            await _alerts.AddAsync(alert, ct);

        await _readings.SaveChangesAsync(ct);

        return new ConsumptionReadingResultViewModel
        {
            ReadingId = reading.Id,
            ConsumerUnitId = unit.Id,
            ReadingTimeUtc = reading.ReadingTimeUtc,
            VolumeLiters = reading.VolumeLiters,
            FlowRateLpm = reading.FlowRateLpm,
            LeakDetected = alert is not null,
            LeakAlertId = alert?.Id,
            LeakSeverity = alert?.Severity.ToString()
        };
    }

    private static LeakAlert? TryBuildLeakAlert(ConsumerUnit unit, ConsumptionReading reading, DateTime now)
    {
        var threshold = unit.IdleFlowThresholdLpm;
        if (threshold <= 0 || reading.FlowRateLpm <= threshold)
            return null;

        var ratio = reading.FlowRateLpm / threshold;
        var severity = ratio switch
        {
            >= 5.0 => LeakSeverity.Critical,
            >= 3.0 => LeakSeverity.High,
            >= 1.8 => LeakSeverity.Medium,
            _ => LeakSeverity.Low
        };

        var excessLpm = reading.FlowRateLpm - threshold;
        var estimatedWaste = Math.Round(excessLpm * WasteWindowMinutes, 2);

        return new LeakAlert
        {
            ConsumerUnitId = unit.Id,
            DetectedAtUtc = now,
            Severity = severity,
            Status = LeakAlertStatus.Open,
            TriggeringFlowRateLpm = reading.FlowRateLpm,
            EstimatedWasteLiters = estimatedWaste,
            Message = $"Flow {reading.FlowRateLpm:F1} L/min is {ratio:F1}x the idle baseline " +
                      $"({threshold:F1} L/min) for unit {unit.Code}."
        };
    }

    public async Task<PagedResult<ConsumptionReadingViewModel>> ListAsync(ConsumptionReadingFilter filter, CancellationToken ct = default)
    {
        var query = _readings.Query();

        if (filter.ConsumerUnitId.HasValue)
            query = query.Where(x => x.ConsumerUnitId == filter.ConsumerUnitId);
        if (filter.FromUtc.HasValue)
            query = query.Where(x => x.ReadingTimeUtc >= filter.FromUtc);
        if (filter.ToUtc.HasValue)
            query = query.Where(x => x.ReadingTimeUtc <= filter.ToUtc);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(x => x.ReadingTimeUtc)
            .Skip(filter.Skip)
            .Take(filter.PageSize)
            .Select(x => new ConsumptionReadingViewModel
            {
                Id = x.Id,
                ConsumerUnitId = x.ConsumerUnitId,
                ConsumerUnitCode = x.ConsumerUnit!.Code,
                ReadingTimeUtc = x.ReadingTimeUtc,
                VolumeLiters = x.VolumeLiters,
                FlowRateLpm = x.FlowRateLpm
            })
            .ToListAsync(ct);

        return new PagedResult<ConsumptionReadingViewModel>(items, filter.Page, filter.PageSize, total);
    }
}
