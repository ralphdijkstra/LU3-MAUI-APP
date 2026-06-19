using ClockApp.Domain.Entities;
using ClockApp.Domain.Enums;
using ClockApp.Domain.ValueObjects;

namespace ClockApp.Domain.Repositories;

public interface IQrCodeRepository
{
    CheckInMode GetMode(int organisationId);

    string GetLocationId(int organisationId);

    void SetMode(int organisationId, CheckInMode mode, string? locationId = null);

    QrPairingSnapshot? CreatePairingCode(int organisationId);

    QrValidationResult ValidatePairingCode(int organisationId, string code, string? locationId = null);
}
