using ClockApp.Application.Interfaces;
using ClockApp.Domain.Entities;

namespace ClockApp.Application.UseCases.Manager;

public class GetClockedInUsersUseCase
{
    private readonly IClockedInPresenceService _presence;

    public GetClockedInUsersUseCase(IClockedInPresenceService presence)
    {
        _presence = presence;
    }

    public async Task<IReadOnlyList<ClockedInUser>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        await _presence.ReconcileLocalSessionAsync(cancellationToken);

        return await _presence.GetAllForOrganisationAsync(cancellationToken);
    }
}
