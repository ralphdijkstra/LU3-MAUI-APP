namespace ClockApp.Domain.Entities;

public sealed class QrPairingSnapshot
{
    public int OrganisationId { get; init; }

    public string Code { get; init; } = string.Empty;

    public string LocationId { get; init; } = string.Empty;

    public DateTime ValidFromUtc { get; init; }

    public DateTime ValidUntilUtc { get; init; }

    public byte[] QrImagePng { get; init; } = [];
}
