using HidroSense.Api.Models;

namespace HidroSense.Api.ViewModels;

public class LeakAlertViewModel
{
    public long Id { get; set; }
    public int ConsumerUnitId { get; set; }
    public string ConsumerUnitCode { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public DateTime DetectedAtUtc { get; set; }
    public string Severity { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public double TriggeringFlowRateLpm { get; set; }
    public double EstimatedWasteLiters { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime? ResolvedAtUtc { get; set; }
    public string? ResolvedBy { get; set; }
}

public class LeakAlertFilter : PaginationQuery
{
    public LeakAlertStatus? Status { get; set; }
    public LeakSeverity? MinSeverity { get; set; }
    public string? Region { get; set; }
}

public class ResolveLeakAlertViewModel
{
    public string? Note { get; set; }
    public bool Dismiss { get; set; }
}
