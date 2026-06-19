namespace ClockApp.Infrastructure.Persistence;

public sealed class ClockedInUsersSnapshot
{
    public List<ClockedInUserSnapshot> Users { get; set; } = [];
}
