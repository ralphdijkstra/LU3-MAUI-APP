using ClockApp.Domain.Enums;

namespace ClockApp.Application.Interfaces;
public interface IClockInModeCache
{
    CheckInMode? Mode { get; set; }

    string? LocationId { get; set; }

    void Clear();
}
