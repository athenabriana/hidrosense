namespace HidroSense.Api.Models;

public class ConsumptionReading
{
    public long Id { get; set; }

    public int ConsumerUnitId { get; set; }
    public ConsumerUnit? ConsumerUnit { get; set; }

    public DateTime ReadingTimeUtc { get; set; }

    public double VolumeLiters { get; set; }

    public double FlowRateLpm { get; set; }

    public DateTime CreatedAt { get; set; }
}
