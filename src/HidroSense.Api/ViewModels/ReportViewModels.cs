namespace HidroSense.Api.ViewModels;

public class RegionImpactViewModel
{
    public string Region { get; set; } = string.Empty;
    public int ConsumerUnits { get; set; }
    public double TotalConsumedLiters { get; set; }
    public double TotalBudgetLiters { get; set; }
    public double BudgetUsagePercent { get; set; }
    public int OpenLeakAlerts { get; set; }
    public double EstimatedWasteLiters { get; set; }
    public string MitigationPlan { get; set; } = string.Empty;
}

public class WaterImpactReportViewModel
{
    public DateTime FromUtc { get; set; }
    public DateTime ToUtc { get; set; }
    public double TotalConsumedLiters { get; set; }
    public double TotalEstimatedWasteLiters { get; set; }
    public double OverallWastePercent { get; set; }
    public IReadOnlyList<RegionImpactViewModel> Regions { get; set; } = Array.Empty<RegionImpactViewModel>();
}

public class WaterImpactReportQuery
{
    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
}
