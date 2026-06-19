using ClockApp.Domain.Enums;

namespace ClockApp.Infrastructure.Persistence;

public class TimeEntrySnapshot
{
    public Guid Id { get; set; }

    public DateTime Timestamp { get; set; }

    public TimeEntryType Type { get; set; }

    public bool IsOfflineEntry { get; set; }

    public SyncStatus SyncStatus { get; set; }
}
