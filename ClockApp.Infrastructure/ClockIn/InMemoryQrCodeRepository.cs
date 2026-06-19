using System.Collections.Concurrent;
using ClockApp.Application.Interfaces;
using ClockApp.Domain;
using ClockApp.Domain.Entities;
using ClockApp.Domain.Enums;
using ClockApp.Domain.Repositories;
using ClockApp.Domain.ValueObjects;

namespace ClockApp.Infrastructure.ClockIn;

public sealed class InMemoryQrCodeRepository : IQrCodeRepository
{
    private readonly ConcurrentDictionary<int, OrganisationQrSettings> _settings = new();
    private readonly QrImageGenerator _imageGenerator;
    private readonly int _windowMinutes;

    public InMemoryQrCodeRepository(QrImageGenerator imageGenerator, IConfiguration configuration)
    {
        _imageGenerator = imageGenerator;
        _windowMinutes = configuration.QrWindowMinutes;
    }

    public CheckInMode GetMode(int organisationId) =>
        _settings.GetOrAdd(organisationId, _ => new OrganisationQrSettings()).Mode;

    public string GetLocationId(int organisationId) =>
        _settings.GetOrAdd(organisationId, _ => new OrganisationQrSettings()).LocationId;

    public void SetMode(int organisationId, CheckInMode mode, string? locationId = null)
    {
        var settings = _settings.GetOrAdd(organisationId, _ => new OrganisationQrSettings());
        settings.Mode = mode;

        if (!string.IsNullOrWhiteSpace(locationId))
            settings.LocationId = locationId.Trim();
    }

    public QrPairingSnapshot? CreatePairingCode(int organisationId)
    {
        if (organisationId <= 0)
            return null;

        var utcNow = DateTime.UtcNow;
        var windowStart = QrPairingCode.GetWindowStart(utcNow, _windowMinutes);
        var code = QrPairingCode.Generate(organisationId, windowStart);

        return new QrPairingSnapshot
        {
            OrganisationId = organisationId,
            Code = code,
            LocationId = GetLocationId(organisationId),
            ValidFromUtc = windowStart,
            ValidUntilUtc = QrPairingCode.GetWindowEnd(windowStart, _windowMinutes),
            QrImagePng = _imageGenerator.CreatePng(code)
        };
    }

    public QrValidationResult ValidatePairingCode(int organisationId, string code, string? locationId = null)
    {
        if (organisationId <= 0)
            return Invalid("Organisatie kon niet worden bepaald. Log opnieuw in.");

        if (!QrPairingCode.TryNormalize(code, out var normalizedCode, out var formatError))
            return Invalid(formatError);

        var utcNow = DateTime.UtcNow;
        var windowStart = QrPairingCode.GetWindowStart(utcNow, _windowMinutes);

        if (!QrPairingCode.IsActive(utcNow, windowStart, _windowMinutes))
            return Invalid("Deze koppelcode is verlopen.");

        var expectedLocation = GetLocationId(organisationId);

        if (!string.IsNullOrWhiteSpace(locationId) && !string.Equals(locationId.Trim(), expectedLocation, StringComparison.OrdinalIgnoreCase))
            return Invalid("Deze koppelcode hoort niet bij deze locatie.");

        var expectedCode = QrPairingCode.Generate(organisationId, windowStart);

        if (!string.Equals(normalizedCode, expectedCode, StringComparison.Ordinal))
            return Invalid("De koppelcode is ongeldig of verlopen.");

        return new QrValidationResult
        {
            IsValid = true,
            Message = string.Empty,
            LocationId = expectedLocation,
            ValidUntilUtc = QrPairingCode.GetWindowEnd(windowStart, _windowMinutes)
        };
    }

    private static QrValidationResult Invalid(string message) => new()
    {
        IsValid = false,
        Message = message
    };

    private sealed class OrganisationQrSettings
    {
        public CheckInMode Mode { get; set; } = CheckInMode.Self;

        public string LocationId { get; set; } = "breda-hogeschool";
    }
}
