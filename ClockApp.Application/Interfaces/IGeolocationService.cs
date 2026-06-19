using ClockApp.Application.Models;

namespace ClockApp.Application.Interfaces;

public interface IGeolocationService
{
    Task<GeolocationReading?> GetCurrentPositionAsync(CancellationToken cancellationToken = default);
}