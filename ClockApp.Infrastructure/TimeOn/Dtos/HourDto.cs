using System.Text.Json.Serialization;

namespace ClockApp.Infrastructure.TimeOn.Dtos;

public class HourDto
{
    [JsonPropertyName("hourID")]
    public int? HourId { get; set; }
}
