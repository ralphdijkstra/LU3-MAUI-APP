using ClockApp.Application.Interfaces;
using ClockApp.Application.Models;
using ClockApp.Infrastructure.Http;
using ClockApp.Infrastructure.TimeOn.Client;
using ClockApp.Infrastructure.TimeOn.Dtos;

namespace ClockApp.Maui.Services;

public class AuthService : IAuthService
{
    private const string AccessTokenKey = "timeon_access_token";
    private const string RefreshTokenKey = "timeon_refresh_token";
    private const string ExpiresAtKey = "timeon_expires_at";

    private readonly AuthClient _authClient;
    private readonly IUserContext _userContext;

    public AuthService(AuthClient authClient, IUserContext userContext)
    {
        _authClient = authClient;
        _userContext = userContext;
    }

    public async Task<bool> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _authClient.LoginWithPasswordAsync(request.Username, request.Password, cancellationToken);

        return await StoreSessionFromResponseAsync(response, cancellationToken);
    }

    public async Task<bool> LoginWithApiTokenAsync(ApiTokenLoginRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _authClient.LoginWithApiTokenAsync(request.ApiToken, cancellationToken);

        return await StoreSessionFromResponseAsync(response, cancellationToken);
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        var session = await LoadSessionAsync(cancellationToken);

        if (session != null && !string.IsNullOrWhiteSpace(session.AccessToken))
            await _authClient.LogoutAsync(session.AccessToken, cancellationToken);

        await ClearSessionAsync();
        await _userContext.ClearAsync(cancellationToken);
    }

    public async Task<bool> IsAuthenticatedAsync(CancellationToken cancellationToken = default)
    {
        var session = await LoadSessionAsync(cancellationToken);

        return session != null && !session.IsExpired;
    }

    public async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var session = await LoadSessionAsync(cancellationToken);

        if (session == null || session.IsExpired)
            return null;

        return session.AccessToken;
    }

    private async Task<bool> StoreSessionFromResponseAsync(ApiResponse<TokenResponseDto> response, CancellationToken cancellationToken)
    {
        if (!response.IsSuccess || response.Data == null)
            return false;

        await SaveSessionAsync(new AuthToken
        {
            AccessToken = response.Data.AccessToken!,
            RefreshToken = response.Data.RefreshToken,
            ExpiresAt = DateTime.UtcNow.AddSeconds(response.Data.ExpiresIn)
        }, cancellationToken);

        return true;
    }

    private async Task<AuthToken?> LoadSessionAsync(CancellationToken cancellationToken)
    {
        var accessToken = await SecureStorage.Default.GetAsync(AccessTokenKey);

        if (string.IsNullOrWhiteSpace(accessToken))
            return null;

        var expiresAtTicks = Preferences.Get(ExpiresAtKey, 0L);

        return new AuthToken
        {
            AccessToken = accessToken,
            RefreshToken = await SecureStorage.Default.GetAsync(RefreshTokenKey),
            ExpiresAt = new DateTime(expiresAtTicks, DateTimeKind.Utc)
        };
    }

    private async Task SaveSessionAsync(AuthToken token, CancellationToken cancellationToken)
    {
        await SecureStorage.Default.SetAsync(AccessTokenKey, token.AccessToken);

        if (!string.IsNullOrWhiteSpace(token.RefreshToken))
            await SecureStorage.Default.SetAsync(RefreshTokenKey, token.RefreshToken);

        Preferences.Set(ExpiresAtKey, token.ExpiresAt.Ticks);
    }

    private Task ClearSessionAsync()
    {
        SecureStorage.Default.Remove(AccessTokenKey);
        SecureStorage.Default.Remove(RefreshTokenKey);
        Preferences.Remove(ExpiresAtKey);

        return Task.CompletedTask;
    }
}
