using ClockApp.Application.Models;

namespace ClockApp.Application.Interfaces;

public interface IAuthService
{
    Task<bool> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<bool> LoginWithApiTokenAsync(ApiTokenLoginRequest request, CancellationToken cancellationToken = default);

    Task LogoutAsync(CancellationToken cancellationToken = default);

    Task<bool> IsAuthenticatedAsync(CancellationToken cancellationToken = default);

    Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}
