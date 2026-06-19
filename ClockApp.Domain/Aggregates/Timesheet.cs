using ClockApp.Domain.Entities;
using ClockApp.Domain.Enums;
using ClockApp.Domain.Exceptions;
using ClockApp.Domain.ValueObjects;

namespace ClockApp.Domain.Aggregates;

public class Timesheet
{
    private readonly List<TimeEntry> _entries;

    public IReadOnlyCollection<TimeEntry> Entries => _entries.AsReadOnly();

    public Timesheet()
    {
        _entries = [];
    }

    public void LoadEntry(TimeEntry entry)
    {
        _entries.Add(entry);
    }

    public TimeEntry CheckIn(DateTime timestamp, bool isOffline)
    {
        if (IsSessionActive())
            throw new DomainException("Medewerker is al ingeklokt.");

        var entry = new TimeEntry(timestamp, TimeEntryType.CheckIn, isOffline);

        _entries.Add(entry);

        return entry;
    }

    public TimeEntry CheckOut(DateTime timestamp, bool isOffline)
    {
        if (!IsSessionActive())
            throw new DomainException("Medewerker moet eerst inklokken.");

        var entry = new TimeEntry(timestamp, TimeEntryType.CheckOut, isOffline);

        _entries.Add(entry);

        return entry;
    }

    public TimeEntry ForceEndBreak(DateTime timestamp, bool isOffline)
    {
        if (!IsOnBreak())
            throw new DomainException("Je bent niet op pauze.");

        var entry = new TimeEntry(timestamp, TimeEntryType.BreakEnd, isOffline);

        _entries.Add(entry);

        return entry;
    }

    public TimeEntry StartBreak(DateTime timestamp, bool isOffline)
    {
        if (!IsWorking())
            throw new DomainException("Je kunt alleen pauze nemen tijdens een actieve werkdag.");

        var entry = new TimeEntry(timestamp, TimeEntryType.BreakStart, isOffline);

        _entries.Add(entry);

        return entry;
    }

    public TimeEntry EndBreak(DateTime timestamp, bool isOffline, TimeSpan minimumBreakDuration)
    {
        if (!IsOnBreak())
            throw new DomainException("Je bent niet op pauze.");

        var breakStart = GetActiveBreakStart()!;
        var elapsed = timestamp - breakStart.Timestamp;

        if (elapsed < minimumBreakDuration)
        {
            var remaining = minimumBreakDuration - elapsed;
            var minutes = (int)Math.Ceiling(remaining.TotalMinutes);

            throw new DomainException($"Minimale pauzetijd van {minimumBreakDuration.TotalMinutes} minuten: nog {minutes} minuut/minuten te gaan.");
        }

        var entry = new TimeEntry(timestamp, TimeEntryType.BreakEnd, isOffline);

        _entries.Add(entry);

        return entry;
    }

    public bool IsSessionActive()
    {
        var last = GetLastEntry();

        return last?.Type is TimeEntryType.CheckIn or TimeEntryType.BreakStart or TimeEntryType.BreakEnd;
    }

    public bool IsOnBreak() => GetLastEntry()?.Type == TimeEntryType.BreakStart;

    public bool IsWorking() => IsSessionActive() && !IsOnBreak();

    public TimeEntry? GetActiveCheckIn()
    {
        if (!IsSessionActive())
            return null;

        return _entries.Where(e => e.Type == TimeEntryType.CheckIn).OrderBy(e => e.Timestamp).LastOrDefault();
    }

    public TimeEntry? GetActiveBreakStart()
    {
        if (!IsOnBreak())
            return null;

        return _entries.Where(e => e.Type == TimeEntryType.BreakStart).OrderBy(e => e.Timestamp).LastOrDefault();
    }

    public TimeSpan GetTotalBreakTime(DateTime now)
    {
        var checkIn = GetActiveCheckIn();

        if (checkIn == null)
            return TimeSpan.Zero;

        var sessionEntries = _entries.Where(e => e.Timestamp >= checkIn.Timestamp);
        var breakSeconds = CalculateBreakSeconds(sessionEntries, IsOnBreak() ? now : null);

        return TimeSpan.FromSeconds(breakSeconds);
    }

    public IEnumerable<TimeEntry> GetPendingEntries() => _entries.Where(e => e.IsPendingSync);

    public IEnumerable<TimeEntry> GetPendingCheckOuts() =>
        _entries.Where(e => e.Type == TimeEntryType.CheckOut && e.IsPendingSync).OrderBy(e => e.Timestamp);

    public int GetPendingHourSyncCount() => GetPendingCheckOuts().Count();

    public bool TryBuildHourRecord(TimeEntry checkOutEntry, out HourRecord? record)
    {
        record = null;

        if (checkOutEntry.Type != TimeEntryType.CheckOut)
            return false;

        if (!TryGetSessionCheckIn(checkOutEntry, out var checkIn))
            return false;

        var sessionEntries = GetSessionEntries(checkIn, checkOutEntry).ToList();
        var totalSeconds = (int)(checkOutEntry.Timestamp - checkIn.Timestamp).TotalSeconds;
        var breakSeconds = CalculateBreakSeconds(sessionEntries);
        var localStart = checkIn.Timestamp.ToLocalTime();

        record = new HourRecord
        {
            Date = DateOnly.FromDateTime(localStart),
            FromSeconds = (int)localStart.TimeOfDay.TotalSeconds,
            Seconds = Math.Max(0, totalSeconds - breakSeconds),
            BreakSeconds = breakSeconds
        };

        return true;
    }

    public IEnumerable<TimeEntry> GetSessionEntries(TimeEntry checkOutEntry)
    {
        if (!TryGetSessionCheckIn(checkOutEntry, out var checkIn))
            return [];

        return GetSessionEntries(checkIn, checkOutEntry);
    }

    private IEnumerable<TimeEntry> GetSessionEntries(TimeEntry checkIn, TimeEntry checkOutEntry) =>
        _entries.Where(e => e.Timestamp >= checkIn.Timestamp && e.Timestamp <= checkOutEntry.Timestamp).OrderBy(e => e.Timestamp);

    private bool TryGetSessionCheckIn(TimeEntry checkOutEntry, out TimeEntry checkIn)
    {
        checkIn = null!;

        TimeEntry? sessionCheckIn = null;

        foreach (var entry in _entries.OrderBy(e => e.Timestamp))
        {
            if (entry.Type == TimeEntryType.CheckIn)
                sessionCheckIn = entry;
            else if (entry.Type == TimeEntryType.CheckOut)
            {
                if (entry.Id == checkOutEntry.Id)
                {
                    if (sessionCheckIn == null)
                        return false;

                    checkIn = sessionCheckIn;

                    return true;
                }

                sessionCheckIn = null;
            }
        }

        return false;
    }

    private static int CalculateBreakSeconds(IEnumerable<TimeEntry> sessionEntries, DateTime? now = null)
    {
        var total = TimeSpan.Zero;
        DateTime? breakStart = null;

        foreach (var entry in sessionEntries.OrderBy(e => e.Timestamp))
        {
            if (entry.Type == TimeEntryType.BreakStart)
                breakStart = entry.Timestamp;
            else if (entry.Type == TimeEntryType.BreakEnd && breakStart.HasValue)
            {
                total += entry.Timestamp - breakStart.Value;
                breakStart = null;
            }
        }

        if (breakStart.HasValue && now.HasValue)
            total += now.Value - breakStart.Value;

        return (int)total.TotalSeconds;
    }

    private TimeEntry? GetLastEntry() => _entries.OrderBy(x => x.Timestamp).LastOrDefault();
}
