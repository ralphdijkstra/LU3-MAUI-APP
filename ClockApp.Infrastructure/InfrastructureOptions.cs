namespace ClockApp.Infrastructure;

public sealed class InfrastructureOptions
{
    public string TimeOnBaseUrl { get; set; } = "https://api.test.timeon.nl/";

    public string StorageRootPath { get; set; } = string.Empty;
}
