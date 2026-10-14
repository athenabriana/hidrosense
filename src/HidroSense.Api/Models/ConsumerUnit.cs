using System.ComponentModel.DataAnnotations;

namespace HidroSense.Api.Models;

public class ConsumerUnit
{
    public int Id { get; set; }

    [MaxLength(40)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    public ConsumerUnitType Type { get; set; }

    [MaxLength(120)]
    public string Region { get; set; } = string.Empty;

    public double IdleFlowThresholdLpm { get; set; }

    public double MonthlyBudgetLiters { get; set; }

    public bool Active { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public ICollection<ConsumptionReading> Readings { get; set; } = new List<ConsumptionReading>();
    public ICollection<LeakAlert> LeakAlerts { get; set; } = new List<LeakAlert>();
}
