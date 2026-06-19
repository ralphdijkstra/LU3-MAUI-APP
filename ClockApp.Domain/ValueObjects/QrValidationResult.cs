namespace ClockApp.Domain.ValueObjects;

public sealed class QrValidationResult
{
    public bool IsValid { get; init; }

    public string Message { get; init; } = string.Empty;

    public string? LocationId { get; init; }

    public DateTime? ValidUntilUtc { get; init; }
}
