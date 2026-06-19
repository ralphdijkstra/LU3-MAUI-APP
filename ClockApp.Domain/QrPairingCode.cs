namespace ClockApp.Domain;

public static class QrPairingCode
{
    public const int Length = 5;

    private const int SeedXor = 0x5A17C0DE;

    public static DateTime GetWindowStart(DateTime utcNow, int windowMinutes)
    {
        var totalMinutes = (int)(utcNow - DateTime.UnixEpoch).TotalMinutes;
        var windowIndex = totalMinutes / windowMinutes;

        return DateTime.UnixEpoch.AddMinutes(windowIndex * windowMinutes);
    }

    public static DateTime GetWindowEnd(DateTime windowStart, int windowMinutes) =>
        windowStart.AddMinutes(windowMinutes);

    public static string Generate(int organisationId, DateTime windowStart)
    {
        var seed = windowStart.Ticks ^ organisationId ^ SeedXor;
        var random = new Random(unchecked((int)seed));
        Span<char> chars = stackalloc char[Length];

        for (var i = 0; i < Length; i++)
            chars[i] = (char)('A' + random.Next(26));

        return new string(chars);
    }

    public static bool TryNormalize(string? code, out string normalized, out string errorMessage)
    {
        normalized = string.Empty;
        errorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(code))
        {
            errorMessage = "Enter a pairing code.";

            return false;
        }

        normalized = code.Trim().ToUpperInvariant();

        if (normalized.Length != Length || normalized.Any(c => c < 'A' || c > 'Z'))
        {
            errorMessage = "The pairing code must consist of 5 letters.";

            return false;
        }

        return true;
    }

    public static bool IsActive(DateTime utcNow, DateTime windowStart, int windowMinutes)
    {
        var windowEnd = GetWindowEnd(windowStart, windowMinutes);

        return utcNow >= windowStart && utcNow < windowEnd;
    }
}
