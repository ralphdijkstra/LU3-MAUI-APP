using ClockApp.Domain.Repositories;
using ClockApp.Infrastructure.ClockIn;
using ClockApp.Infrastructure.Http;
using ClockApp.Infrastructure.Persistence;
using ClockApp.Infrastructure.TimeOn;
using ClockApp.Infrastructure.TimeOn.Client;
using Microsoft.Extensions.DependencyInjection;

namespace ClockApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, Action<InfrastructureOptions>? configure = null)
    {
        var options = new InfrastructureOptions();
        configure?.Invoke(options);

        if (string.IsNullOrWhiteSpace(options.StorageRootPath))
            throw new InvalidOperationException("StorageRootPath is required.");

        var timeOnUri = new Uri(options.TimeOnBaseUrl.TrimEnd('/') + "/");

        services.AddSingleton<ITimeEntryRepository>(_ => new TimeEntryRepository(options.StorageRootPath));
        services.AddSingleton<ITimesheetRepository, TimesheetRepository>();
        services.AddSingleton<IClockedInUserRepository>(_ => new ClockedInUserRepository(options.StorageRootPath));

        services.AddSingleton<QrImageGenerator>();
        services.AddSingleton<IQrCodeRepository, InMemoryQrCodeRepository>();
        services.AddSingleton<IWorkLocationRepository, InMemoryWorkLocationRepository>();

        services.AddScoped<IHourRepository, TimeOnHourRepository>();
        services.AddScoped<IUserRepository, TimeOnUserRepository>();
        services.AddHttpClient<AuthClient>(client => client.BaseAddress = timeOnUri);

        services.AddTransient<BearerTokenHandler>();

        services.AddHttpClient<TimeOnHourClient>(client => client.BaseAddress = timeOnUri)
                .AddHttpMessageHandler<BearerTokenHandler>();

        services.AddHttpClient<TimeOnUserClient>(client => client.BaseAddress = timeOnUri)
                .AddHttpMessageHandler<BearerTokenHandler>();

        return services;
    }
}
