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
        Message = "Pauze gestart en gesynchroniseerd."
    };

    public static StartBreakResult SavedOffline(DateTime breakStartedAt) => new()
    {
        Status = StartBreakStatus.SavedOffline,
        BreakStartedAt = breakStartedAt,
        Message = "Pauze offline opgeslagen. Wordt gesynchroniseerd bij uitklokken."
    };

    public static StartBreakResult PendingSync(DateTime breakStartedAt) => new()
    {
        Status = StartBreakStatus.PendingSync,
        BreakStartedAt = breakStartedAt,
        Message = "Pauze gestart. Synchronisatie volgt zodra de verbinding hersteld is."
    };

    public static StartBreakResult NotWorking() => new()
    {
        Status = StartBreakStatus.NotWorking,
        Message = "Je moet ingeklokt zijn om pauze te nemen."
    };

    public static StartBreakResult Failed(string message) => new()
    {
        Status = StartBreakStatus.Failed,
        Message = message
    };
}
