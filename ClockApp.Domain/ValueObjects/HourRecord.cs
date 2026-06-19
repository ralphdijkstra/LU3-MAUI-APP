namespace ClockApp.Domain.ValueObjects;

public class HourRecord
{
    public int? HourId { get; init; }

    public DateOnly Date { get; init; }

    public int FromSeconds { get; init; }

    public int Seconds { get; init; }

    public int BreakSeconds { get; init; }
}
