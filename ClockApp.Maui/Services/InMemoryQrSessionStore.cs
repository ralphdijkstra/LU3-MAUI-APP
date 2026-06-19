namespace ClockApp.Maui.Services;

public sealed class InMemoryQrSessionStore
{
    public string? ValidatedCode { get; set; }

    public string? LocationId { get; set; }

    public DateTime? ValidatedAtUtc { get; set; }

    public void Clear()
    {
        ValidatedCode = null;
        LocationId = null;
        ValidatedAtUtc = null;
    }
}
