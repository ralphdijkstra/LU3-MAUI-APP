using ClockApp.Application.Interfaces;
using ClockApp.Domain.Entities;
using ClockApp.Domain.Repositories;

namespace ClockApp.Application.Services;

public sealed class ClockedInPresenceService : IClockedInPresenceService
{
    private readonly IClockedInUserRepository _repository;
    private readonly ITimesheetRepository _timesheetRepository;
    private readonly IUserContext _userContext;

    public ClockedInPresenceService(IClockedInUserRepository repository, ITimesheetRepository timesheetRepository, IUserContext userContext)
    {
        _repository = repository;
        _timesheetRepository = timesheetRepository;
        _userContext = userContext;
    }

    public async Task RegisterCheckInAsync(DateTime checkedInAt, string? locationId = null, CancellationToken cancellationToken = default)
    {
        var user = _userContext.Current;

        if (user == null)
            return;

        await _repository.AddOrUpdateAsync(new ClockedInUser
        {
            UserId = user.UserId,
            OrganisationId = user.OrganisationId,
            DisplayName = user.DisplayName,
            CheckedInAt = checkedInAt,
            IsOnBreak = false,
            LocationId = locationId
        }, cancellationToken);
    }

    public async Task RegisterCheckOutAsync(CancellationToken cancellationToken = default)
    {
        var user = _userContext.Current;

        if (user == null)
            return;

        await _repository.RemoveAsync(user.UserId, user.OrganisationId, cancellationToken);
    }

    public async Task SetOnBreakAsync(bool isOnBreak, CancellationToken cancellationToken = default)
    {
        var user = _userContext.Current;

        if (user == null)
            return;

        var users = await _repository.GetAllForOrganisationAsync(user.OrganisationId, cancellationToken);
        var existing = users.FirstOrDefault(u => u.UserId == user.UserId);

        if (existing == null)
            return;

        await _repository.AddOrUpdateAsync(new ClockedInUser
        {
            UserId = existing.UserId,
            OrganisationId = existing.OrganisationId,
            DisplayName = existing.DisplayName,
            CheckedInAt = existing.CheckedInAt,
            IsOnBreak = isOnBreak,
            LocationId = existing.LocationId
        }, cancellationToken);
    }

    public async Task ReconcileLocalSessionAsync(CancellationToken cancellationToken = default)
    {
        var user = _userContext.Current;

        if (user == null)
            return;

        var timesheet = await _timesheetRepository.GetCurrentAsync(cancellationToken);

        if (timesheet == null || !timesheet.IsSessionActive())
        {
            await _repository.RemoveAsync(user.UserId, user.OrganisationId, cancellationToken);

            return;
        }

        var checkedInAt = timesheet.GetActiveCheckIn()?.Timestamp ?? DateTime.UtcNow;

        await _repository.AddOrUpdateAsync(new ClockedInUser
        {
            UserId = user.UserId,
            OrganisationId = user.OrganisationId,
            DisplayName = user.DisplayName,
            CheckedInAt = checkedInAt,
            IsOnBreak = timesheet.IsOnBreak(),
            LocationId = null
        }, cancellationToken);
    }

    public async Task<IReadOnlyList<ClockedInUser>> GetAllForOrganisationAsync(CancellationToken cancellationToken = default)
    {
        var organisationId = _userContext.Current?.OrganisationId ?? 0;

        if (organisationId <= 0)
            return Array.Empty<ClockedInUser>();

        return await _repository.GetAllForOrganisationAsync(organisationId, cancellationToken);
    }
}
