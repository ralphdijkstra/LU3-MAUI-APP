using ClockApp.Domain.Entities;

namespace ClockApp.Application.Interfaces;

public interface IClockedInPresenceService
{
    Task RegisterCheckInAsync(DateTime checkedInAt, string? locationId = null, CancellationToken cancellationToken = default);

    Task RegisterCheckOutAsync(CancellationToken cancellationToken = default);

    Task SetOnBreakAsync(bool isOnBreak, CancellationToken cancellationToken = default);

    Task ReconcileLocalSessionAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClockedInUser>> GetAllForOrganisationAsync(CancellationToken cancellationToken = default);
}
