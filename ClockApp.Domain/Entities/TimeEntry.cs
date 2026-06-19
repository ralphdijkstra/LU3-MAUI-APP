using ClockApp.Domain.Enums;

namespace ClockApp.Domain.Entities;

public class TimeEntry
{
    public Guid Id { get; }

    public DateTime Timestamp { get; }

    public TimeEntryType Type { get; }

    public bool IsOfflineEntry { get; }

    public SyncStatus SyncStatus { get; private set; }

    protected TimeEntry() { }

    public TimeEntry(DateTime timestamp, TimeEntryType type, bool isOfflineEntry)
    {
        Id = Guid.NewGuid();
        Timestamp = timestamp;
        Type = type;
        IsOfflineEntry = isOfflineEntry;
        SyncStatus = SyncStatus.Pending;
    }

    private TimeEntry(Guid id, DateTime timestamp, TimeEntryType type, bool isOfflineEntry, SyncStatus syncStatus)
    {
        Id = id;
        Timestamp = timestamp;
        Type = type;
        IsOfflineEntry = isOfflineEntry;
        SyncStatus = syncStatus;
    }

    public static TimeEntry Reconstitute(Guid id, DateTime timestamp, TimeEntryType type, bool isOfflineEntry, SyncStatus syncStatus) =>
        new(id, timestamp, type, isOfflineEntry, syncStatus);

    public void MarkSynced()
    {
        SyncStatus = SyncStatus.Synced;
    }

    public bool IsPendingSync => SyncStatus == SyncStatus.Pending;
}
