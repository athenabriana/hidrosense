using HidroSense.Api.Data;
using HidroSense.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace HidroSense.Api.Repositories;

public interface IConsumerUnitRepository
{
    IQueryable<ConsumerUnit> Query();
    Task<ConsumerUnit?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct = default);
    Task AddAsync(ConsumerUnit unit, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

public interface IConsumptionRepository
{
    IQueryable<ConsumptionReading> Query();
    Task AddAsync(ConsumptionReading reading, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

public interface ILeakAlertRepository
{
    IQueryable<LeakAlert> Query();
    Task<LeakAlert?> GetByIdAsync(long id, CancellationToken ct = default);
    Task AddAsync(LeakAlert alert, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

public class ConsumerUnitRepository : IConsumerUnitRepository
{
    private readonly AppDbContext _db;
    public ConsumerUnitRepository(AppDbContext db) => _db = db;

    public IQueryable<ConsumerUnit> Query() => _db.ConsumerUnits.AsNoTracking();
    public Task<ConsumerUnit?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.ConsumerUnits.FirstOrDefaultAsync(x => x.Id == id, ct);
    public Task<bool> CodeExistsAsync(string code, CancellationToken ct = default)
        => _db.ConsumerUnits.AnyAsync(x => x.Code == code, ct);
    public async Task AddAsync(ConsumerUnit unit, CancellationToken ct = default)
        => await _db.ConsumerUnits.AddAsync(unit, ct);
    public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}

public class ConsumptionRepository : IConsumptionRepository
{
    private readonly AppDbContext _db;
    public ConsumptionRepository(AppDbContext db) => _db = db;

    public IQueryable<ConsumptionReading> Query() => _db.ConsumptionReadings.AsNoTracking();
    public async Task AddAsync(ConsumptionReading reading, CancellationToken ct = default)
        => await _db.ConsumptionReadings.AddAsync(reading, ct);
    public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}

public class LeakAlertRepository : ILeakAlertRepository
{
    private readonly AppDbContext _db;
    public LeakAlertRepository(AppDbContext db) => _db = db;

    public IQueryable<LeakAlert> Query() => _db.LeakAlerts.AsNoTracking();
    public Task<LeakAlert?> GetByIdAsync(long id, CancellationToken ct = default)
        => _db.LeakAlerts.FirstOrDefaultAsync(x => x.Id == id, ct);
    public async Task AddAsync(LeakAlert alert, CancellationToken ct = default)
        => await _db.LeakAlerts.AddAsync(alert, ct);
    public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
