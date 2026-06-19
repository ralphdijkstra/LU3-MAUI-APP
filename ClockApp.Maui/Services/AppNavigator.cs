using ClockApp.Application.Interfaces;
using ClockApp.Domain.Enums;
using ClockApp.Domain.Repositories;
using ClockApp.Maui.Pages;
using Microsoft.Extensions.DependencyInjection;

namespace ClockApp.Maui.Services;

public class AppNavigator : IAppNavigator
{
    private readonly IServiceProvider _services;
    private readonly IClockInModeCache _modeCache;
    private readonly InMemoryQrSessionStore _qrSessionStore;
    private readonly IUserContext _userContext;

    public AppNavigator(IServiceProvider services, IClockInModeCache modeCache, InMemoryQrSessionStore qrSessionStore, IUserContext userContext)
    {
        _services = services;
        _modeCache = modeCache;
        _qrSessionStore = qrSessionStore;
        _userContext = userContext;
    }

    public Task GoToMainAsync() => NavigateToMainAsync(forceNavigation: true);

    public Task RefreshMainAsync() => NavigateToMainAsync(forceNavigation: false);

    public Task GoToLoginAsync()
    {
        _modeCache.Clear();
        _qrSessionStore.Clear();

        var page = _services.GetRequiredService<LoginPage>();

        SetRoot(page);

        return Task.CompletedTask;
    }

    private async Task NavigateToMainAsync(bool forceNavigation)
    {
        if (_userContext.Current?.OrganisationId <= 0)
            await _userContext.RefreshAsync();

        using var scope = _services.CreateScope();
        var qrCodeRepository = scope.ServiceProvider.GetRequiredService<IQrCodeRepository>();
        var organisationId = _userContext.Current?.OrganisationId ?? 0;
        var mode = organisationId > 0 ? qrCodeRepository.GetMode(organisationId) : CheckInMode.Self;
        var locationId = organisationId > 0 ? qrCodeRepository.GetLocationId(organisationId) : string.Empty;
        var previousMode = _modeCache.Mode;

        _modeCache.Mode = mode;
        _modeCache.LocationId = locationId;

        if (!forceNavigation && previousMode == mode && GetRootPage() is MainTabPage)
            return;

        SetRoot(_services.GetRequiredService<MainTabPage>());
    }

    private static Page? GetRootPage()
    {
        var window = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault();
        var page = window?.Page;

        if (page is NavigationPage navigationPage)
            return navigationPage.RootPage;

        return page;
    }

    private static void SetRoot(Page page)
    {
        var window = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault();

        if (window == null)
            return;

        window.Page = page is NavigationPage or MainTabPage ? page : new NavigationPage(page);
    }
}
