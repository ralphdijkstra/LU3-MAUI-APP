using ClockApp.Domain.Entities;
using ClockApp.Domain.ValueObjects;

namespace ClockApp.Domain.Repositories;

public interface IHourRepository
{
    Task<bool> SaveAsync(HourRecord hour, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<HourEntry>> ListUserDayAsync(DateOnly date, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<HourEntry>> ListAllDayAsync(DateOnly date, int organisationId, CancellationToken cancellationToken = default);
}
