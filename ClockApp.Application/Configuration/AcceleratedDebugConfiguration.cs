namespace ClockApp.Application.Configuration;

public sealed class AcceleratedDebugConfiguration : ConfigurationBase
{
    private readonly double _factor;

    public AcceleratedDebugConfiguration(double factor)
    {
        _factor = factor;
    }

    public override double TimeAccelerationFactor => _factor;
}
