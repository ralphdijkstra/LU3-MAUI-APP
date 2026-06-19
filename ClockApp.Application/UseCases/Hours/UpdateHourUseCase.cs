using ClockApp.Domain.Repositories;
using ClockApp.Domain.ValueObjects;

namespace ClockApp.Application.UseCases.Hours;

public class UpdateHourRequest
{
    public int HourId { get; init; }

    public DateOnly Date { get; init; }

    public int FromSeconds { get; init; }

    public int Seconds { get; init; }

    public int BreakSeconds { get; init; }
}

public class UpdateHourResult
{
    public bool Success { get; init; }

    public string? ErrorMessage { get; init; }

    public static UpdateHourResult Succeeded() => new() { Success = true };

    public static UpdateHourResult Failed(string message) => new() { Success = false, ErrorMessage = message };
}

public class UpdateHourUseCase
{
    private readonly IHourRepository _hourRepository;

    public UpdateHourUseCase(IHourRepository hourRepository)
    {
        _hourRepository = hourRepository;
    }

    public async Task<UpdateHourResult> ExecuteAsync(UpdateHourRequest request, CancellationToken cancellationToken = default)
    {
        if (request.HourId <= 0)
            return UpdateHourResult.Failed("Ongeldige uurregistratie.");

        if (request.Seconds <= 0)
            return UpdateHourResult.Failed("Werkduur moet groter zijn dan nul.");

        var hour = new HourRecord
        {
            HourId = request.HourId,
            Date = request.Date,
            FromSeconds = request.FromSeconds,
            Seconds = request.Seconds,
            BreakSeconds = request.BreakSeconds
        };

        var saved = await _hourRepository.SaveAsync(hour, cancellationToken);

        return saved ? UpdateHourResult.Succeeded() : UpdateHourResult.Failed("Opslaan mislukt. Controleer je verbinding en probeer opnieuw.");
    }
}
