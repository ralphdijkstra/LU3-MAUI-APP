using ClockApp.Application.Interfaces;
using ClockApp.Domain.Repositories;

namespace ClockApp.Application.UseCases.Location;

public class CheckWorkLocationUseCase
{
    private readonly IGeolocationService _geolocation;
    private readonly IWorkLocationRepository _workLocations;
    private readonly IQrCodeRepository _checkInSettings;
    private readonly IUserContext _userContext;

    public CheckWorkLocationUseCase(IGeolocationService geolocation, IWorkLocationRepository workLocations, IQrCodeRepository checkInSettings, IUserContext userContext)
    {
        _geolocation = geolocation;
        _workLocations = workLocations;
        _checkInSettings = checkInSettings;
        _userContext = userContext;
    }

    public async Task<WorkLocationCheckResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var organisationId = await ResolveOrganisationIdAsync(cancellationToken);

        if (organisationId <= 0)
            return WorkLocationCheckResult.NoLocationConfigured();

        var locationId = _checkInSettings.GetLocationId(organisationId);
        var workLocation = _workLocations.GetById(locationId);

        if (workLocation == null)
            return WorkLocationCheckResult.NoLocationConfigured();

        var position = await _geolocation.GetCurrentPositionAsync(cancellationToken);

        if (position == null)
            return WorkLocationCheckResult.LocationUnavailable();

        return workLocation.IsWithinRadius(position.Latitude, position.Longitude)
            ? WorkLocationCheckResult.AtLocation(workLocation.Name)
            : WorkLocationCheckResult.NotAtLocation(workLocation.Name);
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
