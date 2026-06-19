using ClockApp.Application.Interfaces;
using ClockApp.Application.UseCases.Location;
using ClockApp.Application.UseCases.Sync;
using ClockApp.Domain.Enums;
using ClockApp.Domain.Repositories;

namespace ClockApp.Application.UseCases.ClockOut;

public class ClockOutUseCase
{
    private readonly ITimesheetRepository _timesheetRepository;
    private readonly ITimeEntryRepository _timeEntryRepository;
    private readonly IConnectivityService _connectivity;
    private readonly SyncHourSessionUseCase _syncHourSession;
    private readonly IQrCodeRepository _qrCodeRepository;
    private readonly IUserContext _userContext;
    private readonly IClockedInPresenceService _presence;
    private readonly CheckWorkLocationUseCase _checkWorkLocation;

    public ClockOutUseCase(ITimesheetRepository timesheetRepository, ITimeEntryRepository timeEntryRepository, IConnectivityService connectivity, SyncHourSessionUseCase syncHourSession, IQrCodeRepository qrCodeRepository, IUserContext userContext, IClockedInPresenceService presence, CheckWorkLocationUseCase checkWorkLocation)
    {
        _timesheetRepository = timesheetRepository;
        _timeEntryRepository = timeEntryRepository;
        _connectivity = connectivity;
        _syncHourSession = syncHourSession;
        _qrCodeRepository = qrCodeRepository;
        _userContext = userContext;
        _presence = presence;
        _checkWorkLocation = checkWorkLocation;
    }

    public async Task<ClockOutResult> ExecuteAsync(ClockOutRequest request, CancellationToken cancellationToken = default)
    {
        var organisationId = await ResolveOrganisationIdAsync(cancellationToken);
        var mode = organisationId > 0 ? _qrCodeRepository.GetMode(organisationId) : CheckInMode.Self;

        if (mode == CheckInMode.Qr)
        {
            if (string.IsNullOrWhiteSpace(request.QrCode))
                return ClockOutResult.Failed("Scan een QR-code of vul de koppelcode in.");

            var validation = _qrCodeRepository.ValidatePairingCode(organisationId, request.QrCode.Trim(), request.LocationId);

            if (!validation.IsValid)
                return ClockOutResult.Failed(validation.Message ?? "Ongeldige of verlopen QR-code.");
        }

        if (mode == CheckInMode.Gps)
        {
            var locationCheck = await _checkWorkLocation.ExecuteAsync(cancellationToken);

            if (locationCheck.Status == WorkLocationCheckStatus.NoLocationConfigured)
                return ClockOutResult.Failed("Geen bedrijfslocatie gekozen. Vraag je manager om een locatie in te stellen.");

            if (locationCheck.Status == WorkLocationCheckStatus.LocationUnavailable)
                return ClockOutResult.Failed("Locatie niet beschikbaar. Controleer je GPS-instellingen en geef toestemming.");

            if (locationCheck.Status == WorkLocationCheckStatus.NotAtLocation)
                return ClockOutResult.Failed("Je bent niet op locatie. Ga naar de werklocatie om uit te klokken.");
        }

        var timesheet = await _timesheetRepository.GetCurrentAsync(cancellationToken);

        if (timesheet == null || !timesheet.IsSessionActive())
            return ClockOutResult.NotCheckedIn();

        var isOffline = !_connectivity.IsOnline;
        var now = DateTime.UtcNow;

        if (timesheet.IsOnBreak())
        {
            var breakEnd = timesheet.ForceEndBreak(now, isOffline);

            await _timeEntryRepository.SaveEntryAsync(breakEnd, cancellationToken);
        }

        var entry = timesheet.CheckOut(now, isOffline);

        await _timeEntryRepository.SaveEntryAsync(entry, cancellationToken);
        await _presence.RegisterCheckOutAsync(cancellationToken);

        if (isOffline)
            return ClockOutResult.SavedOffline(entry.Timestamp);

        var synced = await _syncHourSession.TrySyncCheckOutAsync(timesheet, entry, cancellationToken);

        if (synced)
            return ClockOutResult.Synced(entry.Timestamp);

        return ClockOutResult.PendingSync(entry.Timestamp);
    }

    private async Task<int> ResolveOrganisationIdAsync(CancellationToken cancellationToken)
    {
        var organisationId = _userContext.Current?.OrganisationId ?? 0;

        if (organisationId > 0)
            return organisationId;

        await _userContext.RefreshAsync(cancellationToken);

        return _userContext.Current?.OrganisationId ?? 0;
    }
}
