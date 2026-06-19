namespace ClockApp.Application.UseCases.Break;

public enum StartBreakStatus
{
    Recorded,
    Synced,
    SavedOffline,
    PendingSync,
    NotWorking,
    Failed
}

public class StartBreakResult
{
    public StartBreakStatus Status { get; init; }

    public DateTime? BreakStartedAt { get; init; }

    public string? Message { get; init; }

    public static StartBreakResult Recorded(DateTime breakStartedAt) => new()
    {
        Status = StartBreakStatus.Recorded,
        BreakStartedAt = breakStartedAt
    };

    public static StartBreakResult Synced(DateTime breakStartedAt) => new()
    {
        Status = StartBreakStatus.Synced,
        BreakStartedAt = breakStartedAt,
        Message = "Break started and synced."
    };

    public static StartBreakResult SavedOffline(DateTime breakStartedAt) => new()
    {
        Status = StartBreakStatus.SavedOffline,
        BreakStartedAt = breakStartedAt,
        Message = "Break saved offline. Will sync when you clock out."
    };

    public static StartBreakResult PendingSync(DateTime breakStartedAt) => new()
    {
        Status = StartBreakStatus.PendingSync,
        BreakStartedAt = breakStartedAt,
        Message = "Break started. Sync will follow once the connection is restored."
    };

    public static StartBreakResult NotWorking() => new()
    {
        Status = StartBreakStatus.NotWorking,
        Message = "You must be clocked in to take a break."
    };

    public static StartBreakResult Failed(string message) => new()
    {
        Status = StartBreakStatus.Failed,
        Message = message
    };
}
