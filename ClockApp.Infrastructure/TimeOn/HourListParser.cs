using System.Globalization;
using System.Text.Json;
using ClockApp.Domain.Entities;

namespace ClockApp.Infrastructure.TimeOn;

public sealed class HourListParseResult
{
    public List<HourEntry> Hours { get; } = new();

    public List<string> PathsToLoad { get; } = new();
}

public static class HourListParser
{
    public static HourListParseResult ParseForDate(JsonElement resultObject, DateOnly date)
    {
        var result = new HourListParseResult();

        if (resultObject.ValueKind != JsonValueKind.Object)
            return result;

        if (resultObject.TryGetProperty("groups", out var groups) && groups.ValueKind == JsonValueKind.Array)
            CollectFromGroups(groups, date, result);

        foreach (var hour in CollectLooseHours(resultObject))
            AddHour(result.Hours, hour);

        return result;
    }

    public static IReadOnlyList<HourEntry> ParseHourItems(JsonElement element, DateOnly? defaultDate = null)
    {
        var hours = new List<HourEntry>();

        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                if (TryParseHour(item, defaultDate, out var hour))
                    hours.Add(hour);
            }

            return hours;
        }

        if (element.ValueKind == JsonValueKind.Object)
        {
            if (element.TryGetProperty("hourList", out var hourList) && hourList.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in hourList.EnumerateArray())
                {
                    if (TryParseHour(item, defaultDate, out var hour))
                        hours.Add(hour);
                }

                return hours;
            }

            foreach (var hour in CollectLooseHours(element))
                hours.Add(hour);
        }

        return hours;
    }

    private static void CollectFromGroups(JsonElement groups, DateOnly date, HourListParseResult result)
    {
        foreach (var group in groups.EnumerateArray())
            CollectFromGroup(group, date, result);
    }

    private static void CollectFromGroup(JsonElement group, DateOnly date, HourListParseResult result)
    {
        if (group.ValueKind != JsonValueKind.Object)
            return;

        var groupDate = TryParseGroupDate(group);
        var isDayGroup = string.Equals(GetString(group, "groupString"), "day", StringComparison.OrdinalIgnoreCase);
        var matchesDate = groupDate == date;

        if (isDayGroup && matchesDate)
        {
            var path = GetString(group, "path");

            if (group.TryGetProperty("hourList", out var hourList) && hourList.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in hourList.EnumerateArray())
                {
                    if (TryParseHour(item, date, out var hour))
                        AddHour(result.Hours, hour);
                }
            }
            else if (!string.IsNullOrWhiteSpace(path) && !result.PathsToLoad.Contains(path))
                result.PathsToLoad.Add(path);
        }

        if (group.TryGetProperty("groups", out var nestedGroups) && nestedGroups.ValueKind == JsonValueKind.Array)
            CollectFromGroups(nestedGroups, date, result);
    }

    private static IEnumerable<HourEntry> CollectLooseHours(JsonElement element)
    {
        var results = new List<HourEntry>();
        WalkForHours(element, results);

        return results;
    }

    private static void WalkForHours(JsonElement element, List<HourEntry> results)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                if (TryParseHour(element, null, out var hour))
                    AddHour(results, hour);

                foreach (var property in element.EnumerateObject())
                    WalkForHours(property.Value, results);

                break;

            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                    WalkForHours(item, results);

                break;
        }
    }

    private static void AddHour(List<HourEntry> hours, HourEntry hour)
    {
        if (hours.Any(existing => existing.HourId == hour.HourId))
            return;

        hours.Add(hour);
    }

    private static bool TryParseHour(JsonElement element, DateOnly? defaultDate, out HourEntry entry)
    {
        entry = null!;

        if (!element.TryGetProperty("hourID", out var hourIdProp) || hourIdProp.ValueKind != JsonValueKind.Number)
            return false;

        var hourId = hourIdProp.GetInt32();

        if (hourId <= 0)
            return false;

        var date = ParseDate(element, defaultDate);
        var fromSeconds = GetInt(element, "fromSeconds");
        var seconds = GetInt(element, "seconds");
        var breakSeconds = GetInt(element, "breakSeconds");
        var userName = GetString(element, "user") ?? string.Empty;
        var userId = GetNullableInt(element, "userID");

        entry = new HourEntry
        {
            HourId = hourId,
            Date = date,
            FromSeconds = fromSeconds,
            Seconds = seconds,
            BreakSeconds = breakSeconds,
            UserName = userName,
            UserId = userId
        };

        return true;
    }

    private static DateOnly? TryParseGroupDate(JsonElement group)
    {
        var shortTitle = GetString(group, "shortTitle");

        if (!string.IsNullOrWhiteSpace(shortTitle) && DateOnly.TryParse(shortTitle, CultureInfo.InvariantCulture, DateTimeStyles.None, out var shortTitleDate))
            return shortTitleDate;

        var groupValue = GetString(group, "groupValue");

        if (groupValue?.StartsWith('D') == true && groupValue.Length == 9 && DateOnly.TryParseExact(groupValue[1..], "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var groupValueDate))
            return groupValueDate;

        return null;
    }

    private static DateOnly ParseDate(JsonElement element, DateOnly? defaultDate)
    {
        var dateString = GetString(element, "date");

        if (!string.IsNullOrWhiteSpace(dateString))
        {
            if (DateOnly.TryParse(dateString.AsSpan(0, Math.Min(10, dateString.Length)), CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                return date;

            if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
                return DateOnly.FromDateTime(dateTime);
        }

        return defaultDate ?? DateOnly.FromDateTime(DateTime.Today);
    }

    private static int GetInt(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.Number)
            return 0;

        return property.TryGetInt32(out var value) ? value : 0;
    }

    private static int? GetNullableInt(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.Number)
            return null;

        return property.TryGetInt32(out var value) ? value : null;
    }

    private static string? GetString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
            return null;

        return property.ValueKind == JsonValueKind.String ? property.GetString() : null;
    }
}
