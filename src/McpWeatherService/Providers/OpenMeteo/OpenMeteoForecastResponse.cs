using System.Text.Json.Serialization;

namespace McpWeatherService.Providers.OpenMeteo;

public sealed class OpenMeteoForecastResponse
{
    [JsonPropertyName("current")]
    public OpenMeteoCurrentWeather? Current { get; init; }
}

public sealed class OpenMeteoCurrentWeather
{
    [JsonPropertyName("time")]
    public string? Time { get; init; }

    [JsonPropertyName("temperature_2m")]
    public decimal? Temperature2M { get; init; }

    [JsonPropertyName("wind_speed_10m")]
    public decimal? WindSpeed10M { get; init; }

    [JsonPropertyName("wind_direction_10m")]
    public int? WindDirection10M { get; init; }

    [JsonPropertyName("weather_code")]
    public int? WeatherCode { get; init; }
}
