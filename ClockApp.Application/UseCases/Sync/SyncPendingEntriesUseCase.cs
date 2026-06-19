using ClockApp.Application.Interfaces;
using ClockApp.Domain.Repositories;

namespace ClockApp.Application.UseCases.Sync;

public class SyncPendingEntriesUseCase
{
    private readonly ITimesheetRepository _repository;
    private readonly IConnectivityService _connectivity;
    private readonly SyncHourSessionUseCase _syncHourSession;

    public SyncPendingEntriesUseCase(ITimesheetRepository repository, IConnectivityService connectivity, SyncHourSessionUseCase syncHourSession)
    {
        _repository = repository;
        _connectivity = connectivity;
        _syncHourSession = syncHourSession;
    }

    public async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (!_connectivity.IsOnline)
            return 0;

        var timesheet = await _repository.GetCurrentAsync(cancellationToken);

        if (timesheet == null)
            return 0;

        var syncedCount = 0;

        foreach (var checkOut in timesheet.GetPendingCheckOuts())
        {
            var success = await _syncHourSession.TrySyncCheckOutAsync(timesheet, checkOut, cancellationToken);

            if (!success)
                break;

            syncedCount++;
        }

        return syncedCount;
    }
}
