namespace ClockApp.Infrastructure.Persistence;

public class TimesheetSnapshot
{
    public List<TimeEntrySnapshot> Entries { get; set; } = [];
}
