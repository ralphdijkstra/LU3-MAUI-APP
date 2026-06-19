using ClockApp.Application.Interfaces;
using ClockApp.Domain.Entities;
using ClockApp.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace ClockApp.Maui.Services;

public class UserContext : IUserContext
{
    private const string UserIdKey = "timeon_user_id";
    private const string OrganisationIdKey = "timeon_user_organisation_id";
    private const string UserNameKey = "timeon_user_name";
    private const string UserEmailKey = "timeon_user_email";
    private const string IsManagerKey = "timeon_user_is_manager";

    private readonly IServiceScopeFactory _scopeFactory;

    public UserContext(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public UserInfo? Current { get; private set; }

    public bool IsManager => Current?.IsManager == true;

    public async Task<bool> RefreshAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var user = await userRepository.GetCurrentUserAsync(cancellationToken);

        if (user == null)
        {
            Current = null;

            return false;
        }

        Current = user;
        SaveCached(user);

        return true;
    }

    public Task LoadCachedAsync(CancellationToken cancellationToken = default)
    {
        var userId = Preferences.Get(UserIdKey, 0);

        if (userId == 0)
        {
            Current = null;

            return Task.CompletedTask;
        }

        Current = new UserInfo
        {
            UserId = userId,
            OrganisationId = Preferences.Get(OrganisationIdKey, 0),
            DisplayName = Preferences.Get(UserNameKey, string.Empty),
            Email = Preferences.Get(UserEmailKey, string.Empty),
            IsManager = Preferences.Get(IsManagerKey, false)
        };

        return Task.CompletedTask;
    }

    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        Current = null;
        Preferences.Remove(UserIdKey);
        Preferences.Remove(OrganisationIdKey);
        Preferences.Remove(UserNameKey);
        Preferences.Remove(UserEmailKey);
        Preferences.Remove(IsManagerKey);

        return Task.CompletedTask;
    }

    private static void SaveCached(UserInfo user)
    {
        Preferences.Set(UserIdKey, user.UserId);
        Preferences.Set(OrganisationIdKey, user.OrganisationId);
        Preferences.Set(UserNameKey, user.DisplayName);
        Preferences.Set(UserEmailKey, user.Email);
        Preferences.Set(IsManagerKey, user.IsManager);
    }
}
