namespace ClockApp.Domain.Entities;

public sealed class UserInfo
{
    public int UserId { get; init; }

    public int OrganisationId { get; init; }

    public string DisplayName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public bool IsManager { get; init; }
}
