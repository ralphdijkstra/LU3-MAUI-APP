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
        Message = "Pauze beëindigd en gesynchroniseerd."
    };

    public static EndBreakResult SavedOffline(DateTime breakEndedAt) => new()
    {
        Status = EndBreakStatus.SavedOffline,
        BreakEndedAt = breakEndedAt,
        Message = "Offline opgeslagen. Wordt gesynchroniseerd bij uitklokken."
    };

    public static EndBreakResult PendingSync(DateTime breakEndedAt) => new()
    {
        Status = EndBreakStatus.PendingSync,
        BreakEndedAt = breakEndedAt,
        Message = "Pauze beëindigd. Synchronisatie volgt zodra de verbinding hersteld is."
    };

    public static EndBreakResult NotOnBreak() => new()
    {
        Status = EndBreakStatus.NotOnBreak,
        Message = "Je bent niet op pauze."
    };

    public static EndBreakResult MinimumDurationNotMet(TimeSpan remaining) => new()
    {
        Status = EndBreakStatus.MinimumDurationNotMet,
        RemainingBreakTime = remaining,
        Message = $"Minimale pauzetijd nog niet bereikt. Nog {Math.Ceiling(remaining.TotalMinutes)} minuut/minuten te gaan."
    };

    public static EndBreakResult Failed(string message) => new()
    {
        Status = EndBreakStatus.Failed,
        Message = message
    };
}
