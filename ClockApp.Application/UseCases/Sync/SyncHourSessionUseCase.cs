using ClockApp.Application.Interfaces;
using ClockApp.Domain.Aggregates;
using ClockApp.Domain.Entities;
using ClockApp.Domain.Repositories;

namespace ClockApp.Application.UseCases.Sync;

public class SyncHourSessionUseCase
{
    private readonly ITimeEntryRepository _timeEntryRepository;
    private readonly IHourRepository _hourRepository;

    public SyncHourSessionUseCase(ITimeEntryRepository timeEntryRepository, IHourRepository hourRepository)
    {
        _timeEntryRepository = timeEntryRepository;
        _hourRepository = hourRepository;
    }

    public async Task<bool> TrySyncCheckOutAsync(Timesheet timesheet, TimeEntry checkOut, CancellationToken cancellationToken = default)
    {
        if (!timesheet.TryBuildHourRecord(checkOut, out var hour) || hour == null)
            return false;

        var saved = await _hourRepository.SaveAsync(hour, cancellationToken);

        if (!saved)
            return false;

        foreach (var entry in timesheet.GetSessionEntries(checkOut))
        {
            entry.MarkSynced();
            await _timeEntryRepository.UpdateEntryAsync(entry, cancellationToken);
        }

        return true;
    }
}
