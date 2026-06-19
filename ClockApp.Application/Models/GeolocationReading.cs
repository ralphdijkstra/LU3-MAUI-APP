namespace ClockApp.Application.Models;

public sealed class GeolocationReading
{
    public double Latitude { get; init; }

    public double Longitude { get; init; }

    public double? AccuracyMeters { get; init; }
}
