using ClockApp.Application.Interfaces;
using ClockApp.Application.UseCases.Location;
using ClockApp.Domain.Enums;
using ClockApp.Domain.Aggregates;
using ClockApp.Domain.Repositories;

namespace ClockApp.Application.UseCases.ClockIn;

public class ClockInUseCase
{
    private readonly ITimesheetRepository _timesheetRepository;
    private readonly ITimeEntryRepository _timeEntryRepository;
    private readonly IConnectivityService _connectivity;
    private readonly IQrCodeRepository _qrCodeRepository;
    private readonly IUserContext _userContext;
    private readonly IClockedInPresenceService _presence;
    private readonly CheckWorkLocationUseCase _checkWorkLocation;

    public ClockInUseCase(ITimesheetRepository timesheetRepository, ITimeEntryRepository timeEntryRepository, IConnectivityService connectivity, IQrCodeRepository qrCodeRepository, IUserContext userContext, IClockedInPresenceService presence, CheckWorkLocationUseCase checkWorkLocation)
    {
        _timesheetRepository = timesheetRepository;
        _timeEntryRepository = timeEntryRepository;
        _connectivity = connectivity;
        _qrCodeRepository = qrCodeRepository;
        _userContext = userContext;
        _presence = presence;
        _checkWorkLocation = checkWorkLocation;
    }

    public async Task<ClockInResult> ExecuteAsync(ClockInRequest request, CancellationToken cancellationToken = default)
    {
        var organisationId = await ResolveOrganisationIdAsync(cancellationToken);
        var mode = organisationId > 0 ? _qrCodeRepository.GetMode(organisationId) : CheckInMode.Self;

        if (mode == CheckInMode.Qr)
        {
            if (string.IsNullOrWhiteSpace(request.QrCode))
                return ClockInResult.Failed("Scan a QR code or enter the pairing code.");

            var validation = _qrCodeRepository.ValidatePairingCode(organisationId, request.QrCode.Trim(), request.LocationId);

            if (!validation.IsValid)
                return ClockInResult.Failed(validation.Message ?? "Invalid or expired QR code.");
        }

        if (mode == CheckInMode.Gps)
        {
            var locationCheck = await _checkWorkLocation.ExecuteAsync(cancellationToken);

            if (locationCheck.Status == WorkLocationCheckStatus.NoLocationConfigured)
                return ClockInResult.Failed("No company location selected. Ask your manager to set a location.");

            if (locationCheck.Status == WorkLocationCheckStatus.LocationUnavailable)
                return ClockInResult.Failed("Location unavailable. Check your GPS settings and grant permission.");

            if (locationCheck.Status == WorkLocationCheckStatus.NotAtLocation)
                return ClockInResult.Failed("You are not on site. Go to the work location to clock in.");
        }

        var timesheet = await _timesheetRepository.GetCurrentAsync(cancellationToken) ?? new Timesheet();

        if (timesheet.IsSessionActive())
            return ClockInResult.AlreadyCheckedIn();

        var isOffline = !_connectivity.IsOnline;
        var entry = timesheet.CheckIn(DateTime.UtcNow, isOffline);

        await _timeEntryRepository.SaveEntryAsync(entry, cancellationToken);
        await _presence.RegisterCheckInAsync(entry.Timestamp, request.LocationId, cancellationToken);

        return isOffline
            ? ClockInResult.SavedOffline(entry.Timestamp)
            : ClockInResult.Recorded(entry.Timestamp);
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
