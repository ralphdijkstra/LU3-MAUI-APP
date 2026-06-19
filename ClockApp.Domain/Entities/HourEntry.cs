namespace ClockApp.Domain.Entities;

public class HourEntry
{
    public int HourId { get; init; }

    public DateOnly Date { get; init; }

    public int FromSeconds { get; init; }

    public int Seconds { get; init; }

    public int BreakSeconds { get; init; }

    public string UserName { get; init; } = string.Empty;

    public int? UserId { get; init; }
}
