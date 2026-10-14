using HidroSense.Api.Middleware;
using HidroSense.Api.Models;
using HidroSense.Api.Repositories;
using HidroSense.Api.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace HidroSense.Api.Services;

public interface IConsumerUnitService
{
    Task<PagedResult<ConsumerUnitViewModel>> ListAsync(ConsumerUnitFilter filter, CancellationToken ct = default);
    Task<ConsumerUnitViewModel> CreateAsync(CreateConsumerUnitViewModel input, CancellationToken ct = default);
}

public class ConsumerUnitService : IConsumerUnitService
{
    private readonly IConsumerUnitRepository _repo;

    public ConsumerUnitService(IConsumerUnitRepository repo)
    {
        _repo = repo;
    }

    public async Task<PagedResult<ConsumerUnitViewModel>> ListAsync(ConsumerUnitFilter filter, CancellationToken ct = default)
    {
        var query = _repo.Query();

        if (!string.IsNullOrWhiteSpace(filter.Region))
            query = query.Where(x => x.Region == filter.Region);
        if (filter.Type.HasValue)
            query = query.Where(x => x.Type == filter.Type);
        if (filter.Active.HasValue)
            query = query.Where(x => x.Active == filter.Active);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(x => x.Id)
            .Skip(filter.Skip)
            .Take(filter.PageSize)
            .Select(x => new ConsumerUnitViewModel
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Type = x.Type.ToString(),
                Region = x.Region,
                IdleFlowThresholdLpm = x.IdleFlowThresholdLpm,
                MonthlyBudgetLiters = x.MonthlyBudgetLiters,
                Active = x.Active,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(ct);

        return new PagedResult<ConsumerUnitViewModel>(items, filter.Page, filter.PageSize, total);
    }

    public async Task<ConsumerUnitViewModel> CreateAsync(CreateConsumerUnitViewModel input, CancellationToken ct = default)
    {
        if (await _repo.CodeExistsAsync(input.Code, ct))
            throw new DomainException($"A consumer unit with code '{input.Code}' already exists.",
                StatusCodes.Status409Conflict);

        var unit = new ConsumerUnit
        {
            Code = input.Code.Trim(),
            Name = input.Name.Trim(),
            Type = input.Type,
            Region = input.Region.Trim(),
            IdleFlowThresholdLpm = input.IdleFlowThresholdLpm,
            MonthlyBudgetLiters = input.MonthlyBudgetLiters,
            Active = true,
            CreatedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(unit, ct);
        await _repo.SaveChangesAsync(ct);

        return new ConsumerUnitViewModel
        {
            Id = unit.Id,
            Code = unit.Code,
            Name = unit.Name,
            Type = unit.Type.ToString(),
            Region = unit.Region,
            IdleFlowThresholdLpm = unit.IdleFlowThresholdLpm,
            MonthlyBudgetLiters = unit.MonthlyBudgetLiters,
            Active = unit.Active,
            CreatedAt = unit.CreatedAt
        };
    }
}
