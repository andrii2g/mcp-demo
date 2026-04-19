using System.Text.Json.Serialization;

namespace McpWeatherService.Providers.OpenMeteo;

public sealed class OpenMeteoGeocodingResponse
{
    [JsonPropertyName("results")]
    public List<OpenMeteoGeocodingResult>? Results { get; init; }
}

public sealed class OpenMeteoGeocodingResult
{
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("country")]
    public string? Country { get; init; }

    [JsonPropertyName("country_code")]
    public string? CountryCode { get; init; }

    [JsonPropertyName("admin1")]
    public string? Admin1 { get; init; }

    [JsonPropertyName("latitude")]
    public double Latitude { get; init; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; init; }

    [JsonPropertyName("timezone")]
    public string? Timezone { get; init; }
}
