using System.Text.Json.Serialization;

namespace ClockApp.Infrastructure.TimeOn.Dtos;

public class HourSaveDto
{
    [JsonPropertyName("hourID")]
    public int? HourId { get; set; }

    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;

    [JsonPropertyName("seconds")]
    public int Seconds { get; set; }

    [JsonPropertyName("fromSeconds")]
    public int FromSeconds { get; set; }

    [JsonPropertyName("breakSeconds")]
    public int? BreakSeconds { get; set; }
}
