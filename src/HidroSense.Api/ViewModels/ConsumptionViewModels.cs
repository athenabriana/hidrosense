namespace HidroSense.Api.ViewModels;

public class CreateConsumptionReadingViewModel
{
    public int ConsumerUnitId { get; set; }
    public DateTime? ReadingTimeUtc { get; set; }
    public double VolumeLiters { get; set; }
    public double FlowRateLpm { get; set; }
}

public class ConsumptionReadingResultViewModel
{
    public long ReadingId { get; set; }
    public int ConsumerUnitId { get; set; }
    public DateTime ReadingTimeUtc { get; set; }
    public double VolumeLiters { get; set; }
    public double FlowRateLpm { get; set; }
    public bool LeakDetected { get; set; }
    public long? LeakAlertId { get; set; }
    public string? LeakSeverity { get; set; }
}

public class ConsumptionReadingViewModel
{
    public long Id { get; set; }
    public int ConsumerUnitId { get; set; }
    public string ConsumerUnitCode { get; set; } = string.Empty;
    public DateTime ReadingTimeUtc { get; set; }
    public double VolumeLiters { get; set; }
    public double FlowRateLpm { get; set; }
}

public class ConsumptionReadingFilter : PaginationQuery
{
    public int? ConsumerUnitId { get; set; }
    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
}
