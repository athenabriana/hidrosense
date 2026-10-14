namespace HidroSense.Api.Models;

public enum ConsumerUnitType
{
    Residential = 0,
    Commercial = 1,
    Industrial = 2,
    Public = 3
}

public enum LeakSeverity
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

public enum LeakAlertStatus
{
    Open = 0,
    Acknowledged = 1,
    Resolved = 2,
    Dismissed = 3
}
