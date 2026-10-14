using HidroSense.Api.Models;

namespace HidroSense.Api.ViewModels;

public class CreateConsumerUnitViewModel
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ConsumerUnitType Type { get; set; }
    public string Region { get; set; } = string.Empty;
    public double IdleFlowThresholdLpm { get; set; }
    public double MonthlyBudgetLiters { get; set; }
}

public class ConsumerUnitViewModel
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public double IdleFlowThresholdLpm { get; set; }
    public double MonthlyBudgetLiters { get; set; }
    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ConsumerUnitFilter : PaginationQuery
{
    public string? Region { get; set; }
    public ConsumerUnitType? Type { get; set; }
    public bool? Active { get; set; }
}
