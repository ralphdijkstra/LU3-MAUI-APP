using System.Text.Json.Serialization;

namespace ClockApp.Infrastructure.TimeOn.Dtos;

public class UserInfoDto
{
    [JsonPropertyName("userID")]
    public int UserId { get; set; }

    [JsonPropertyName("organisationID")]
    public int OrganisationId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("isManager")]
    public bool IsManager { get; set; }

    [JsonPropertyName("isSystemAdmin")]
    public bool IsSystemAdmin { get; set; }

    [JsonPropertyName("allowSettings")]
    public bool AllowSettings { get; set; }
}
