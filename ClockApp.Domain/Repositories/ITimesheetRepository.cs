using ClockApp.Domain.Aggregates;

namespace ClockApp.Domain.Repositories;

public interface ITimesheetRepository
{
    Task<Timesheet?> GetCurrentAsync(CancellationToken cancellationToken = default);
}
