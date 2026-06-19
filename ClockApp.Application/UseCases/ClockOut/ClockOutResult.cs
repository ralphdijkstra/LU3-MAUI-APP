namespace ClockApp.Application.UseCases.ClockOut;

public enum ClockOutStatus
{
    Synced,
    SavedOffline,
    PendingSync,
    NotCheckedIn,
    Failed
}

public class ClockOutResult
{
    public ClockOutStatus Status { get; init; }

    public DateTime? CheckedOutAt { get; init; }

    public string? Message { get; init; }

    public static ClockOutResult Synced(DateTime checkedOutAt) => new()
    {
        Status = ClockOutStatus.Synced,
        CheckedOutAt = checkedOutAt,
        Message = "Clocked out and synced."
    };

    public static ClockOutResult SavedOffline(DateTime checkedOutAt) => new()
    {
        Status = ClockOutStatus.SavedOffline,
        CheckedOutAt = checkedOutAt,
        Message = "Saved offline. Will sync once internet is available."
    };

    public static ClockOutResult PendingSync(DateTime checkedOutAt) => new()
    {
        Status = ClockOutStatus.PendingSync,
        CheckedOutAt = checkedOutAt,
        Message = "Clocked out. Sync will follow once the connection is restored."
    };

    public static ClockOutResult NotCheckedIn() => new()
    {
        Status = ClockOutStatus.NotCheckedIn,
        Message = "You are not clocked in. Clock in before clocking out."
    };

    public static ClockOutResult Failed(string message) => new()
    {
        Status = ClockOutStatus.Failed,
        Message = message
    };
}
