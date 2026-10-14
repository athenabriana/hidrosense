using System.ComponentModel.DataAnnotations;

namespace HidroSense.Api.Models;

public class LeakAlert
{
    public long Id { get; set; }

    public int ConsumerUnitId { get; set; }
    public ConsumerUnit? ConsumerUnit { get; set; }

    public DateTime DetectedAtUtc { get; set; }

    public LeakSeverity Severity { get; set; }

    public LeakAlertStatus Status { get; set; } = LeakAlertStatus.Open;

    public double TriggeringFlowRateLpm { get; set; }

    public double EstimatedWasteLiters { get; set; }

    [MaxLength(300)]
    public string Message { get; set; } = string.Empty;

    public DateTime? ResolvedAtUtc { get; set; }

    [MaxLength(120)]
    public string? ResolvedBy { get; set; }
}
