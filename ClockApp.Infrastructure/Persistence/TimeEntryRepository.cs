using ClockApp.Domain.Entities;
using ClockApp.Domain.Repositories;
using System.Text.Json;

namespace ClockApp.Infrastructure.Persistence;

public sealed class TimeEntryRepository : ITimeEntryRepository
{
    private const string TimesheetFileName = "timesheet.json";

    private readonly string _rootPath;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public TimeEntryRepository(string rootPath)
    {
        _rootPath = rootPath;
    }

    public async Task<IReadOnlyList<TimeEntry>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var snapshot = await ReadSnapshotAsync(cancellationToken);

        if (snapshot == null)
            return [];

        return snapshot.Entries
            .OrderBy(e => e.Timestamp)
            .Select(e => TimeEntry.Reconstitute(e.Id, e.Timestamp, e.Type, e.IsOfflineEntry, e.SyncStatus))
            .ToList();
    }

    public async Task SaveEntryAsync(TimeEntry entry, CancellationToken cancellationToken = default)
    {
        var snapshot = await ReadSnapshotAsync(cancellationToken) ?? new TimesheetSnapshot();

        snapshot.Entries.Add(ToSnapshot(entry));

        await WriteSnapshotAsync(snapshot, cancellationToken);
    }

    public async Task UpdateEntryAsync(TimeEntry entry, CancellationToken cancellationToken = default)
    {
        var snapshot = await ReadSnapshotAsync(cancellationToken);

        if (snapshot == null)
            return;

        var index = snapshot.Entries.FindIndex(e => e.Id == entry.Id);

        if (index < 0)
            return;

        snapshot.Entries[index] = ToSnapshot(entry);

        await WriteSnapshotAsync(snapshot, cancellationToken);
    }

    private async Task<TimesheetSnapshot?> ReadSnapshotAsync(CancellationToken cancellationToken)
    {
        var path = GetFilePath();

        if (!File.Exists(path))
            return null;

        await using var stream = File.OpenRead(path);

        return await JsonSerializer.DeserializeAsync<TimesheetSnapshot>(stream, _jsonOptions, cancellationToken);
    }

    private async Task WriteSnapshotAsync(TimesheetSnapshot snapshot, CancellationToken cancellationToken)
    {
        var path = GetFilePath();
        var directory = Path.GetDirectoryName(path)!;

        Directory.CreateDirectory(directory);

        await using var stream = File.Create(path);

        await JsonSerializer.SerializeAsync(stream, snapshot, _jsonOptions, cancellationToken);
    }

    private string GetFilePath() => Path.Combine(_rootPath, "timesheets", TimesheetFileName);

    private static TimeEntrySnapshot ToSnapshot(TimeEntry entry) => new()
    {
        Id = entry.Id,
        Timestamp = entry.Timestamp,
        Type = entry.Type,
        IsOfflineEntry = entry.IsOfflineEntry,
        SyncStatus = entry.SyncStatus
    };
}
