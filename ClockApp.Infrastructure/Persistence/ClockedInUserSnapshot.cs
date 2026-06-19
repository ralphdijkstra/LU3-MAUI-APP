namespace ClockApp.Infrastructure.Persistence;

public sealed class ClockedInUserSnapshot
{
    public int UserId { get; set; }

    public int OrganisationId { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public DateTime CheckedInAt { get; set; }

    public bool IsOnBreak { get; set; }

    public string? LocationId { get; set; }
}
