using ClockApp.Application.Interfaces;
using ClockApp.Domain.Entities;
using ClockApp.Domain.Repositories;

namespace ClockApp.Application.UseCases.Hours;

public class GetAllDayHoursUseCase
{
    private readonly IHourRepository _hourRepository;
    private readonly IUserContext _userContext;

    public GetAllDayHoursUseCase(IHourRepository hourRepository, IUserContext userContext)
    {
        _hourRepository = hourRepository;
        _userContext = userContext;
    }

    public async Task<IReadOnlyList<HourEntry>> ExecuteAsync(DateOnly? date = null, CancellationToken cancellationToken = default)
    {
        var organisationId = _userContext.Current?.OrganisationId ?? 0;

        if (organisationId <= 0)
            return Array.Empty<HourEntry>();

        return await _hourRepository.ListAllDayAsync(date ?? DateOnly.FromDateTime(DateTime.Today), organisationId, cancellationToken);
    }
}
