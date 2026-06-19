namespace ClockApp.Domain.Entities;

public sealed class WorkLocation
{
    private const double EarthRadiusMeters = 6371000;

    public string Id { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public double Latitude { get; init; }

    public double Longitude { get; init; }

    public int RadiusMeters { get; init; }

    public bool IsWithinRadius(double latitude, double longitude) =>
        DistanceMeters(Latitude, Longitude, latitude, longitude) <= RadiusMeters;

    private static double DistanceMeters(double lat1, double lon1, double lat2, double lon2)
    {
        var dLat = DegreesToRadians(lat2 - lat1);
        var dLon = DegreesToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadiusMeters * c;
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180;
}
