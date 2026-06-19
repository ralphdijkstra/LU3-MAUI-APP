namespace ClockApp.Domain.Entities;

public sealed class ClockedInUser
{
    public int UserId { get; init; }

    public int OrganisationId { get; init; }

    public string DisplayName { get; init; } = string.Empty;

    public DateTime CheckedInAt { get; init; }

    public bool IsOnBreak { get; init; }

    public string? LocationId { get; init; }
}
