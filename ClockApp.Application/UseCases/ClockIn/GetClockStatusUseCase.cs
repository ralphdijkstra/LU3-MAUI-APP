using ClockApp.Application.Interfaces;
using ClockApp.Domain.Repositories;

namespace ClockApp.Application.UseCases.ClockIn;

public class GetClockStatusUseCase
{
    private readonly ITimesheetRepository _repository;
    private readonly IConfiguration _configuration;

    public GetClockStatusUseCase(ITimesheetRepository repository, IConfiguration configuration)
    {
        _repository = repository;
        _configuration = configuration;
    }

    public async Task<ClockStatus> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var timesheet = await _repository.GetCurrentAsync(cancellationToken);

        if (timesheet == null)
        {
            return new ClockStatus
            {
                IsSessionActive = false,
                IsWorking = false,
                IsOnBreak = false,
                PendingSyncCount = 0
            };
        }

        var breakStart = timesheet.GetActiveBreakStart();
        var now = DateTime.UtcNow;
        var remaining = breakStart != null ? _configuration.RemainingBreakTime(breakStart.Timestamp) : (TimeSpan?)null;

        return new ClockStatus
        {
            IsSessionActive = timesheet.IsSessionActive(),
            IsWorking = timesheet.IsWorking(),
            IsOnBreak = timesheet.IsOnBreak(),
            CheckedInAt = timesheet.GetActiveCheckIn()?.Timestamp,
            BreakStartedAt = breakStart?.Timestamp,
            RemainingBreakTime = remaining,
            CanEndBreak = breakStart != null && _configuration.CanEndBreak(breakStart.Timestamp),
            TotalBreakTime = timesheet.GetTotalBreakTime(now),
            PendingSyncCount = timesheet.GetPendingHourSyncCount()
        };
    }
}
