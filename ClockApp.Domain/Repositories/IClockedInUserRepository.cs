using ClockApp.Domain.Entities;

namespace ClockApp.Domain.Repositories;

public interface IClockedInUserRepository
{
    Task<IReadOnlyList<ClockedInUser>> GetAllForOrganisationAsync(int organisationId, CancellationToken cancellationToken = default);

    Task AddOrUpdateAsync(ClockedInUser user, CancellationToken cancellationToken = default);

    Task RemoveAsync(int userId, int organisationId, CancellationToken cancellationToken = default);
}
