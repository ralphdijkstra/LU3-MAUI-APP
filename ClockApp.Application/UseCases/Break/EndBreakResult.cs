namespace ClockApp.Application.UseCases.Break;

public enum EndBreakStatus
{
    Recorded,
    Synced,
    SavedOffline,
    PendingSync,
    NotOnBreak,
    MinimumDurationNotMet,
    Failed
}

public class EndBreakResult
{
    public EndBreakStatus Status { get; init; }

    public DateTime? BreakEndedAt { get; init; }

    public TimeSpan? RemainingBreakTime { get; init; }

    public string? Message { get; init; }

    public static EndBreakResult Recorded(DateTime breakEndedAt) => new()
    {
        Status = EndBreakStatus.Recorded,
        BreakEndedAt = breakEndedAt
    };

    public static EndBreakResult Synced(DateTime breakEndedAt) => new()
    {
        Status = EndBreakStatus.Synced,
        BreakEndedAt = breakEndedAt,
        Message = "Break ended and synced."
    };

    public static EndBreakResult SavedOffline(DateTime breakEndedAt) => new()
    {
        Status = EndBreakStatus.SavedOffline,
        BreakEndedAt = breakEndedAt,
        Message = "Saved offline. Will sync when you clock out."
    };

    public static EndBreakResult PendingSync(DateTime breakEndedAt) => new()
    {
        Status = EndBreakStatus.PendingSync,
        BreakEndedAt = breakEndedAt,
        Message = "Break ended. Sync will follow once the connection is restored."
    };

    public static EndBreakResult NotOnBreak() => new()
    {
        Status = EndBreakStatus.NotOnBreak,
        Message = "You are not on break."
    };

    public static EndBreakResult MinimumDurationNotMet(TimeSpan remaining) => new()
    {
        Status = EndBreakStatus.MinimumDurationNotMet,
        RemainingBreakTime = remaining,
        Message = $"Minimum break time not yet reached. {Math.Ceiling(remaining.TotalMinutes)} minute(s) remaining."
    };

    public static EndBreakResult Failed(string message) => new()
    {
        Status = EndBreakStatus.Failed,
        Message = message
    };
}
