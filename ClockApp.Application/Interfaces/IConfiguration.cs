namespace ClockApp.Application.Interfaces;

public interface IConfiguration
{
    double TimeAccelerationFactor { get; }

    TimeSpan MinimumBreakDuration { get; }

    TimeSpan EffectiveMinimumBreakDuration { get; }

    int QrWindowMinutes { get; }

    bool CanEndBreak(DateTime breakStartedAt);

    TimeSpan RemainingBreakTime(DateTime breakStartedAt);
}
