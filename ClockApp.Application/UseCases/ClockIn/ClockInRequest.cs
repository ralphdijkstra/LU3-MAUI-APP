namespace ClockApp.Application.UseCases.ClockIn;

public class ClockInRequest
{
    public int? CustomerId { get; init; }

    public int? ProjectId { get; init; }

    public string? Description { get; init; }

    public string? LocationId { get; init; }

    public string? QrCode { get; init; }
}
