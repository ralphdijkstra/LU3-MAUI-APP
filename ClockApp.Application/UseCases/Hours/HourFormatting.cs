namespace ClockApp.Application.UseCases.Hours;

public static class HourFormatting
{
    public static string FormatDuration(int totalSeconds)
    {
        if (totalSeconds <= 0)
            return "0:00";

        var time = TimeSpan.FromSeconds(totalSeconds);
        var hours = (int)time.TotalHours;

        return $"{hours}:{time.Minutes:D2}";
    }

    public static string FormatEndTimeOfDay(int fromSeconds, int seconds)
    {
        if (seconds <= 0)
            return "—";

        return FormatTimeOfDay(fromSeconds + seconds);
    }

    public static string FormatTimeRange(int fromSeconds, int seconds)
    {
        if (seconds <= 0)
            return "—";

        var start = TimeSpan.FromSeconds(fromSeconds);
        var end = TimeSpan.FromSeconds(fromSeconds + seconds);

        return $"{start.Hours:D2}:{start.Minutes:D2} – {end.Hours:D2}:{end.Minutes:D2}";
    }

    public static string FormatTimeOfDay(int seconds)
    {
        var time = TimeSpan.FromSeconds(seconds);

        return $"{time.Hours:D2}:{time.Minutes:D2}";
    }

    public static bool TryParseTimeOfDay(string input, out int seconds)
    {
        seconds = 0;

        if (string.IsNullOrWhiteSpace(input))
            return false;

        var parts = input.Trim().Split(':');

        if (parts.Length < 2)
            return false;

        if (!int.TryParse(parts[0], out var hours) || !int.TryParse(parts[1], out var minutes))
            return false;

        if (hours is < 0 or > 23 || minutes is < 0 or > 59)
            return false;

        seconds = hours * 3600 + minutes * 60;

        return true;
    }

    public static bool TryParseDuration(string input, out int seconds)
    {
        seconds = 0;

        if (string.IsNullOrWhiteSpace(input))
            return false;

        var parts = input.Trim().Split(':');

        if (parts.Length < 2)
            return false;

        if (!int.TryParse(parts[0], out var hours) || !int.TryParse(parts[1], out var minutes))
            return false;

        if (hours < 0 || minutes is < 0 or > 59)
            return false;

        seconds = hours * 3600 + minutes * 60;

        return seconds > 0;
    }

    public static bool TryParseBreakDuration(string input, out int seconds)
    {
        seconds = 0;

        if (string.IsNullOrWhiteSpace(input))
            return true;

        var parts = input.Trim().Split(':');

        if (parts.Length < 2)
            return false;

        if (!int.TryParse(parts[0], out var hours) || !int.TryParse(parts[1], out var minutes))
            return false;

        if (hours < 0 || minutes is < 0 or > 59)
            return false;

        seconds = hours * 3600 + minutes * 60;

        return true;
    }
}
