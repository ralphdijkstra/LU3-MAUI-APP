namespace ClockApp.Application.UseCases.ClockIn;

public class ClockStatus
{
    public bool IsSessionActive { get; init; }

    public bool IsWorking { get; init; }

    public bool IsOnBreak { get; init; }

    public DateTime? CheckedInAt { get; init; }

    public DateTime? BreakStartedAt { get; init; }

    public TimeSpan? RemainingBreakTime { get; init; }

    public bool CanEndBreak { get; init; }

    public TimeSpan TotalBreakTime { get; init; }

    public int PendingSyncCount { get; init; }
}
