using ClockApp.Application.Interfaces;
using ClockApp.Domain.Enums;

namespace ClockApp.Maui.Services;

public sealed class InMemoryClockInModeCache : IClockInModeCache
{
    public CheckInMode? Mode { get; set; }

    public string? LocationId { get; set; }

    public void Clear()
    {
        Mode = null;
        LocationId = null;
    }
}
