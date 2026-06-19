using ClockApp.Domain.Entities;

namespace ClockApp.Application.Interfaces;

public interface IUserContext
{
    UserInfo? Current { get; }

    bool IsManager { get; }

    Task<bool> RefreshAsync(CancellationToken cancellationToken = default);

    Task LoadCachedAsync(CancellationToken cancellationToken = default);

    Task ClearAsync(CancellationToken cancellationToken = default);
}
