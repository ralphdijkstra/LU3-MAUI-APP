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
        Message = "Ingeklokt en gesynchroniseerd."
    };

    public static ClockInResult SavedOffline(DateTime checkedInAt) => new()
    {
        Status = ClockInStatus.SavedOffline,
        CheckedInAt = checkedInAt,
        Message = "Offline opgeslagen. Wordt gesynchroniseerd bij uitklokken."
    };

    public static ClockInResult PendingSync(DateTime checkedInAt) => new()
    {
        Status = ClockInStatus.PendingSync,
        CheckedInAt = checkedInAt,
        Message = "Ingeklokt. Synchronisatie volgt zodra de verbinding hersteld is."
    };

    public static ClockInResult AlreadyCheckedIn() => new()
    {
        Status = ClockInStatus.AlreadyCheckedIn,
        Message = "Je bent al ingeklokt."
    };

    public static ClockInResult Failed(string message) => new()
    {
        Status = ClockInStatus.Failed,
        Message = message
    };
}
