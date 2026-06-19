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
        Message = "Uitgeklokt en gesynchroniseerd."
    };

    public static ClockOutResult SavedOffline(DateTime checkedOutAt) => new()
    {
        Status = ClockOutStatus.SavedOffline,
        CheckedOutAt = checkedOutAt,
        Message = "Offline opgeslagen. Wordt gesynchroniseerd zodra er internet is."
    };

    public static ClockOutResult PendingSync(DateTime checkedOutAt) => new()
    {
        Status = ClockOutStatus.PendingSync,
        CheckedOutAt = checkedOutAt,
        Message = "Uitgeklokt. Synchronisatie volgt zodra de verbinding hersteld is."
    };

    public static ClockOutResult NotCheckedIn() => new()
    {
        Status = ClockOutStatus.NotCheckedIn,
        Message = "Je bent niet ingeklokt. Klok eerst in voordat je uitklokt."
    };

    public static ClockOutResult Failed(string message) => new()
    {
        Status = ClockOutStatus.Failed,
        Message = message
    };
}
