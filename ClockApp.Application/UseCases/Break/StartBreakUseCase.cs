using ClockApp.Application.Interfaces;
using ClockApp.Domain.Aggregates;
using ClockApp.Domain.Repositories;

namespace ClockApp.Application.UseCases.Break;

public class StartBreakUseCase
{
    private readonly ITimesheetRepository _timesheetRepository;
    private readonly ITimeEntryRepository _timeEntryRepository;
    private readonly IConnectivityService _connectivity;
    private readonly IClockedInPresenceService _presence;

    public StartBreakUseCase(ITimesheetRepository timesheetRepository, ITimeEntryRepository timeEntryRepository, IConnectivityService connectivity, IClockedInPresenceService presence)
    {
        _timesheetRepository = timesheetRepository;
        _timeEntryRepository = timeEntryRepository;
        _connectivity = connectivity;
        _presence = presence;
    }

    public async Task<StartBreakResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var timesheet = await _timesheetRepository.GetCurrentAsync(cancellationToken) ?? new Timesheet();

        if (!timesheet.IsWorking())
            return StartBreakResult.NotWorking();

        var isOffline = !_connectivity.IsOnline;
        var entry = timesheet.StartBreak(DateTime.UtcNow, isOffline);

        await _timeEntryRepository.SaveEntryAsync(entry, cancellationToken);
        await _presence.SetOnBreakAsync(true, cancellationToken);

        return isOffline
            ? StartBreakResult.SavedOffline(entry.Timestamp)
            : StartBreakResult.Recorded(entry.Timestamp);
    }
}
