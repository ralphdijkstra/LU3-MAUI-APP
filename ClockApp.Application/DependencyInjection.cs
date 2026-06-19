using ClockApp.Application.Configuration;
using ClockApp.Application.Interfaces;
using ClockApp.Application.UseCases.Break;
using ClockApp.Application.UseCases.ClockIn;
using ClockApp.Application.UseCases.ClockOut;
using ClockApp.Application.Services;
using ClockApp.Application.UseCases.Hours;
using ClockApp.Application.UseCases.Location;
using ClockApp.Application.UseCases.Manager;
using ClockApp.Application.UseCases.Sync;
using Microsoft.Extensions.DependencyInjection;

namespace ClockApp.Application;

public static class DependencyInjection
{
    private const double DebugTimeFactor = 60;

    private static readonly TimeSpan MinimumBreakDuration = TimeSpan.FromMinutes(1);

    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        RegisterConfiguration(services);

        services.AddScoped<SyncHourSessionUseCase>();
        services.AddScoped<GetClockStatusUseCase>();
        services.AddScoped<ClockInUseCase>();
        services.AddScoped<ClockOutUseCase>();
        services.AddScoped<StartBreakUseCase>();
        services.AddScoped<EndBreakUseCase>();
        services.AddScoped<SyncPendingEntriesUseCase>();
        services.AddScoped<GetUserDayHoursUseCase>();
        services.AddScoped<GetAllDayHoursUseCase>();
        services.AddScoped<UpdateHourUseCase>();
        services.AddSingleton<IClockedInPresenceService, ClockedInPresenceService>();
        services.AddScoped<GetClockedInUsersUseCase>();
        services.AddScoped<CheckWorkLocationUseCase>();

        return services;
    }

    private static void RegisterConfiguration(IServiceCollection services)
    {
#if DEBUG
        services.AddSingleton<IConfiguration>(new AcceleratedDebugConfiguration(DebugTimeFactor)
        {
            MinimumBreakDuration = MinimumBreakDuration
        });
#else
        services.AddSingleton<IConfiguration>(new SystemConfiguration
        {
            MinimumBreakDuration = MinimumBreakDuration
        });
#endif
    }
}
