using ClockApp.Domain.Entities;
using ClockApp.Domain.Repositories;

namespace ClockApp.Application.UseCases.Hours;

public class GetUserDayHoursUseCase
{
    private readonly IHourRepository _hourRepository;

    public GetUserDayHoursUseCase(IHourRepository hourRepository)
    {
        _hourRepository = hourRepository;
    }

    public Task<IReadOnlyList<HourEntry>> ExecuteAsync(DateOnly? date = null, CancellationToken cancellationToken = default) =>
        _hourRepository.ListUserDayAsync(date ?? DateOnly.FromDateTime(DateTime.Today), cancellationToken);
}
