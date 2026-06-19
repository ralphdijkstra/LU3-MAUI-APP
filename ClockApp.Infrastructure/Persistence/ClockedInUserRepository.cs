using ClockApp.Domain.Entities;
using ClockApp.Domain.Repositories;
using System.Text.Json;

namespace ClockApp.Infrastructure.Persistence;

public sealed class ClockedInUserRepository : IClockedInUserRepository
{
    private const string FileName = "clocked-in-users.json";

    private readonly string _rootPath;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ClockedInUserRepository(string rootPath)
    {
        _rootPath = rootPath;
    }

    public async Task<IReadOnlyList<ClockedInUser>> GetAllForOrganisationAsync(int organisationId, CancellationToken cancellationToken = default)
    {
        var snapshot = await ReadSnapshotAsync(cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return snapshot.Users
            .Where(u => u.OrganisationId == organisationId && DateOnly.FromDateTime(u.CheckedInAt) == today)
            .OrderBy(u => u.DisplayName, StringComparer.OrdinalIgnoreCase)
            .Select(ToEntity)
            .ToList();
    }

    public async Task AddOrUpdateAsync(ClockedInUser user, CancellationToken cancellationToken = default)
    {
        var snapshot = await ReadSnapshotAsync(cancellationToken);
        var index = snapshot.Users.FindIndex(u => u.UserId == user.UserId && u.OrganisationId == user.OrganisationId);

        var entry = ToSnapshot(user);

        if (index < 0)
            snapshot.Users.Add(entry);
        else
            snapshot.Users[index] = entry;

        await WriteSnapshotAsync(snapshot, cancellationToken);
    }

    public async Task RemoveAsync(int userId, int organisationId, CancellationToken cancellationToken = default)
    {
        var snapshot = await ReadSnapshotAsync(cancellationToken);
        var removed = snapshot.Users.RemoveAll(u => u.UserId == userId && u.OrganisationId == organisationId);

        if (removed == 0)
            return;

        await WriteSnapshotAsync(snapshot, cancellationToken);
    }

    private async Task<ClockedInUsersSnapshot> ReadSnapshotAsync(CancellationToken cancellationToken)
    {
        var path = GetFilePath();

        if (!File.Exists(path))
            return new ClockedInUsersSnapshot();

        await using var stream = File.OpenRead(path);

        return await JsonSerializer.DeserializeAsync<ClockedInUsersSnapshot>(stream, _jsonOptions, cancellationToken) ?? new ClockedInUsersSnapshot();
    }

    private async Task WriteSnapshotAsync(ClockedInUsersSnapshot snapshot, CancellationToken cancellationToken)
    {
        var path = GetFilePath();
        var directory = Path.GetDirectoryName(path)!;

        Directory.CreateDirectory(directory);

        await using var stream = File.Create(path);

        await JsonSerializer.SerializeAsync(stream, snapshot, _jsonOptions, cancellationToken);
    }

    private string GetFilePath() => Path.Combine(_rootPath, "presence", FileName);

    private static ClockedInUser ToEntity(ClockedInUserSnapshot snapshot) => new()
    {
        UserId = snapshot.UserId,
        OrganisationId = snapshot.OrganisationId,
        DisplayName = snapshot.DisplayName,
        CheckedInAt = snapshot.CheckedInAt,
        IsOnBreak = snapshot.IsOnBreak,
        LocationId = snapshot.LocationId
    };

    private static ClockedInUserSnapshot ToSnapshot(ClockedInUser user) => new()
    {
        UserId = user.UserId,
        OrganisationId = user.OrganisationId,
        DisplayName = user.DisplayName,
        CheckedInAt = user.CheckedInAt,
        IsOnBreak = user.IsOnBreak,
        LocationId = user.LocationId
    };
}
