namespace ClockApp.Application.UseCases.Location;

public enum WorkLocationCheckStatus
{
    AtLocation,
    NotAtLocation,
    LocationUnavailable,
    NoLocationConfigured
}

public sealed class WorkLocationCheckResult
{
    public WorkLocationCheckStatus Status { get; init; }

    public string? LocationName { get; init; }

    public static WorkLocationCheckResult AtLocation(string locationName) => new()
    {
        Status = WorkLocationCheckStatus.AtLocation,
        LocationName = locationName
    };

    public static WorkLocationCheckResult NotAtLocation(string? locationName = null) => new()
    {
        Status = WorkLocationCheckStatus.NotAtLocation,
        LocationName = locationName
    };

    public static WorkLocationCheckResult LocationUnavailable() => new()
    {
        Status = WorkLocationCheckStatus.LocationUnavailable
    };

    public static WorkLocationCheckResult NoLocationConfigured() => new()
    {
        Status = WorkLocationCheckStatus.NoLocationConfigured
    };
}
