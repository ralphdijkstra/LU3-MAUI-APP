using ClockApp.Application.Interfaces;
using ClockApp.Domain.Aggregates;
using ClockApp.Domain.Repositories;

namespace ClockApp.Application.UseCases.Break;

public class EndBreakUseCase
{
    private readonly ITimesheetRepository _timesheetRepository;
    private readonly ITimeEntryRepository _timeEntryRepository;
    private readonly IConnectivityService _connectivity;
    private readonly IConfiguration _configuration;
    private readonly IClockedInPresenceService _presence;

    public EndBreakUseCase(ITimesheetRepository timesheetRepository, ITimeEntryRepository timeEntryRepository, IConnectivityService connectivity, IConfiguration configuration, IClockedInPresenceService presence)
    {
        _timesheetRepository = timesheetRepository;
        _timeEntryRepository = timeEntryRepository;
        _connectivity = connectivity;
        _configuration = configuration;
        _presence = presence;
    }

    public async Task<EndBreakResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var timesheet = await _timesheetRepository.GetCurrentAsync(cancellationToken);

        if (timesheet == null || !timesheet.IsOnBreak())
            return EndBreakResult.NotOnBreak();

        var breakStart = timesheet.GetActiveBreakStart()!;

        if (!_configuration.CanEndBreak(breakStart.Timestamp))
            return EndBreakResult.MinimumDurationNotMet(_configuration.RemainingBreakTime(breakStart.Timestamp));

        var isOffline = !_connectivity.IsOnline;
        var now = DateTime.UtcNow;
        var entry = timesheet.EndBreak(now, isOffline, _configuration.EffectiveMinimumBreakDuration);

        await _timeEntryRepository.SaveEntryAsync(entry, cancellationToken);
        await _presence.SetOnBreakAsync(false, cancellationToken);

        return isOffline
            ? EndBreakResult.SavedOffline(entry.Timestamp)
            : EndBreakResult.Recorded(entry.Timestamp);
    }
}
