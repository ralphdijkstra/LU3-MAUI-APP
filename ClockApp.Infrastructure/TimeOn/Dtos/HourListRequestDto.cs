using System.Text.Json.Serialization;

namespace ClockApp.Infrastructure.TimeOn.Dtos;

public class HourListRequestDto
{
    [JsonPropertyName("filter")]
    public HourListFilterDto Filter { get; set; } = new();
}

public class HourListFilterDto
{
    [JsonPropertyName("toString")]
    public string? ToStringValue { get; set; }

    [JsonPropertyName("preventLargeDataset")]
    public bool PreventLargeDataset { get; set; } = false;

    [JsonPropertyName("groupString")]
    public string? GroupString { get; set; }

    [JsonPropertyName("period")]
    public string? Period { get; set; }

    [JsonPropertyName("from")]
    public string? From { get; set; }

    [JsonPropertyName("to")]
    public string? To { get; set; }

    [JsonPropertyName("context")]
    public string? Context { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("paged")]
    public bool? Paged { get; set; }

    [JsonPropertyName("userID")]
    public int? UserId { get; set; }

    [JsonPropertyName("deleted")]
    public bool Deleted { get; set; }

    [JsonPropertyName("organisationID")]
    public int? OrganisationId { get; set; }
}
