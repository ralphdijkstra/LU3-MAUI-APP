using ClockApp.Domain.Entities;

namespace ClockApp.Domain.Repositories;

public interface ITimeEntryRepository
{
    Task<IReadOnlyList<TimeEntry>> GetAllAsync(CancellationToken cancellationToken = default);

    Task SaveEntryAsync(TimeEntry entry, CancellationToken cancellationToken = default);

    Task UpdateEntryAsync(TimeEntry entry, CancellationToken cancellationToken = default);
}
