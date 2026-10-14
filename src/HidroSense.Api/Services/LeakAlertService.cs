using HidroSense.Api.Middleware;
using HidroSense.Api.Models;
using HidroSense.Api.Repositories;
using HidroSense.Api.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace HidroSense.Api.Services;

public interface ILeakAlertService
{
    Task<PagedResult<LeakAlertViewModel>> ListAsync(LeakAlertFilter filter, CancellationToken ct = default);
    Task<LeakAlertViewModel> ResolveAsync(long id, string resolvedBy, ResolveLeakAlertViewModel input, CancellationToken ct = default);
}

public class LeakAlertService : ILeakAlertService
{
    private readonly ILeakAlertRepository _repo;

    public LeakAlertService(ILeakAlertRepository repo)
    {
        _repo = repo;
    }

    public async Task<PagedResult<LeakAlertViewModel>> ListAsync(LeakAlertFilter filter, CancellationToken ct = default)
    {
        var query = _repo.Query();

        if (filter.Status.HasValue)
            query = query.Where(x => x.Status == filter.Status);
        if (filter.MinSeverity.HasValue)
            query = query.Where(x => x.Severity >= filter.MinSeverity);
        if (!string.IsNullOrWhiteSpace(filter.Region))
            query = query.Where(x => x.ConsumerUnit!.Region == filter.Region);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(x => x.Severity)
            .ThenByDescending(x => x.DetectedAtUtc)
            .Skip(filter.Skip)
            .Take(filter.PageSize)
            .Select(x => new LeakAlertViewModel
            {
                Id = x.Id,
                ConsumerUnitId = x.ConsumerUnitId,
                ConsumerUnitCode = x.ConsumerUnit!.Code,
                Region = x.ConsumerUnit!.Region,
                DetectedAtUtc = x.DetectedAtUtc,
                Severity = x.Severity.ToString(),
                Status = x.Status.ToString(),
                TriggeringFlowRateLpm = x.TriggeringFlowRateLpm,
                EstimatedWasteLiters = x.EstimatedWasteLiters,
                Message = x.Message,
                ResolvedAtUtc = x.ResolvedAtUtc,
                ResolvedBy = x.ResolvedBy
            })
            .ToListAsync(ct);

        return new PagedResult<LeakAlertViewModel>(items, filter.Page, filter.PageSize, total);
    }

    public async Task<LeakAlertViewModel> ResolveAsync(long id, string resolvedBy, ResolveLeakAlertViewModel input, CancellationToken ct = default)
    {
        var alert = await _repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Leak alert {id} was not found.");

        if (alert.Status is LeakAlertStatus.Resolved or LeakAlertStatus.Dismissed)
            throw new DomainException($"Leak alert {id} is already {alert.Status}.",
                StatusCodes.Status409Conflict);

        alert.Status = input.Dismiss ? LeakAlertStatus.Dismissed : LeakAlertStatus.Resolved;
        alert.ResolvedAtUtc = DateTime.UtcNow;
        alert.ResolvedBy = resolvedBy;
        if (!string.IsNullOrWhiteSpace(input.Note))
            alert.Message = $"{alert.Message} | Resolution: {input.Note}";

        await _repo.SaveChangesAsync(ct);

        return new LeakAlertViewModel
        {
            Id = alert.Id,
            ConsumerUnitId = alert.ConsumerUnitId,
            ConsumerUnitCode = alert.ConsumerUnit?.Code ?? string.Empty,
            Region = alert.ConsumerUnit?.Region ?? string.Empty,
            DetectedAtUtc = alert.DetectedAtUtc,
            Severity = alert.Severity.ToString(),
            Status = alert.Status.ToString(),
            TriggeringFlowRateLpm = alert.TriggeringFlowRateLpm,
            EstimatedWasteLiters = alert.EstimatedWasteLiters,
            Message = alert.Message,
            ResolvedAtUtc = alert.ResolvedAtUtc,
            ResolvedBy = alert.ResolvedBy
        };
    }
}
