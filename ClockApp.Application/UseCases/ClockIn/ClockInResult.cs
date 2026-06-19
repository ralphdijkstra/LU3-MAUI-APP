namespace ClockApp.Application.UseCases.ClockIn;

public enum ClockInStatus
{
    Recorded,
    Synced,
    SavedOffline,
    PendingSync,
    AlreadyCheckedIn,
    Failed
}

public class ClockInResult
{
    public ClockInStatus Status { get; init; }

    public DateTime? CheckedInAt { get; init; }

    public string? Message { get; init; }

    public static ClockInResult Recorded(DateTime checkedInAt) => new()
    {
        Status = ClockInStatus.Recorded,
        CheckedInAt = checkedInAt
    };

    public static ClockInResult Synced(DateTime checkedInAt) => new()
    {
        Status = ClockInStatus.Synced,
        CheckedInAt = checkedInAt,
        Message = "Clocked in and synced."
    };

    public static ClockInResult SavedOffline(DateTime checkedInAt) => new()
    {
        Status = ClockInStatus.SavedOffline,
        CheckedInAt = checkedInAt,
        Message = "Saved offline. Will sync when you clock out."
    };

    public static ClockInResult PendingSync(DateTime checkedInAt) => new()
    {
        Status = ClockInStatus.PendingSync,
        CheckedInAt = checkedInAt,
        Message = "Clocked in. Sync will follow once the connection is restored."
    };

    public static ClockInResult AlreadyCheckedIn() => new()
    {
        Status = ClockInStatus.AlreadyCheckedIn,
        Message = "You are already clocked in."
    };

    public static ClockInResult Failed(string message) => new()
    {
        Status = ClockInStatus.Failed,
        Message = message
    };
}
