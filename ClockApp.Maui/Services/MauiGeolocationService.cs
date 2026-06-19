using ClockApp.Application.Interfaces;
using ClockApp.Application.Models;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;

namespace ClockApp.Maui.Services;

public sealed class MauiGeolocationService : IGeolocationService
{
    public async Task<GeolocationReading?> GetCurrentPositionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            if (status != PermissionStatus.Granted)
                return null;

            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(12));
            var location = await Geolocation.Default.GetLocationAsync(request, cancellationToken);

            if (location == null)
                return null;

            return new GeolocationReading
            {
                Latitude = location.Latitude,
                Longitude = location.Longitude,
                AccuracyMeters = location.Accuracy
            };
        }
        catch (FeatureNotSupportedException)
        {
            return null;
        }
        catch (PermissionException)
        {
            return null;
        }
    }
}
