using ClockApp.Domain.Aggregates;
using ClockApp.Domain.Repositories;

namespace ClockApp.Infrastructure.Persistence;

public sealed class TimesheetRepository : ITimesheetRepository
{
    private readonly ITimeEntryRepository _entries;

    public TimesheetRepository(ITimeEntryRepository entries)
    {
        _entries = entries;
    }

    public async Task<Timesheet?> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        var entries = await _entries.GetAllAsync(cancellationToken);

        if (entries.Count == 0)
            return null;

        var timesheet = new Timesheet();

        foreach (var entry in entries)
            timesheet.LoadEntry(entry);

        return timesheet;
    }
}
