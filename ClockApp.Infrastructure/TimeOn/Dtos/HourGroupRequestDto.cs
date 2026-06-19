using System.Text.Json.Serialization;

namespace ClockApp.Infrastructure.TimeOn.Dtos;

public class HourGroupRequestDto
{
    [JsonPropertyName("filter")]
    public HourListFilterDto Filter { get; set; } = new();

    [JsonPropertyName("hourGroupKey")]
    public string? HourGroupKey { get; set; }
}
