using ClockApp.Application.Interfaces;

namespace ClockApp.Application.Configuration;

public abstract class ConfigurationBase : IConfiguration
{
    public TimeSpan MinimumBreakDuration { get; init; } = TimeSpan.FromMinutes(1);

    public int QrWindowMinutes { get; init; } = 10;

    public virtual double TimeAccelerationFactor => 1;

    public TimeSpan EffectiveMinimumBreakDuration =>
        TimeSpan.FromTicks((long)(MinimumBreakDuration.Ticks / TimeAccelerationFactor));

    public bool CanEndBreak(DateTime breakStartedAt) =>
        DateTime.UtcNow - breakStartedAt >= EffectiveMinimumBreakDuration;

    public TimeSpan RemainingBreakTime(DateTime breakStartedAt)
    {
        var remaining = EffectiveMinimumBreakDuration - (DateTime.UtcNow - breakStartedAt);

        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }
}
