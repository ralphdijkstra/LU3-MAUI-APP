using ClockApp.Application;
using ClockApp.Application.Interfaces;
using ClockApp.Infrastructure;
using ClockApp.Maui.Pages;
using ClockApp.Maui.Services;
using ClockApp.Maui.ViewModels;
using Microsoft.Extensions.Logging;
using ZXing.Net.Maui.Controls;

namespace ClockApp.Maui;

public static class DependencyInjection
{
    public static MauiAppBuilder AddPresentation(this MauiAppBuilder builder)
    {
        builder
            .UseMauiApp<App>()
            .UseBarcodeReader()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton<IAuthService, AuthService>();
        builder.Services.AddSingleton<IUserContext, UserContext>();
        builder.Services.AddSingleton<IConnectivityService, MauiConnectivityService>();
        builder.Services.AddSingleton<IGeolocationService, MauiGeolocationService>();
        builder.Services.AddSingleton<IAppNavigator, AppNavigator>();
        builder.Services.AddSingleton<IClockInModeCache, InMemoryClockInModeCache>();
        builder.Services.AddSingleton<InMemoryQrSessionStore>();

        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<ClockViewModel>();
        builder.Services.AddTransient<ClockPage>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<HoursViewModel>();
        builder.Services.AddTransient<HoursPage>();
        builder.Services.AddTransient<MainTabPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder;
    }

    public static MauiAppBuilder AddApplication(this MauiAppBuilder builder)
    {
        builder.Services.AddApplication();

        return builder;
    }

    public static MauiAppBuilder AddInfrastructure(this MauiAppBuilder builder, Action<InfrastructureOptions>? configure = null)
    {
        builder.Services.AddInfrastructure(options =>
        {
            configure?.Invoke(options);
            options.StorageRootPath = FileSystem.AppDataDirectory;
        });

        return builder;
    }
}
