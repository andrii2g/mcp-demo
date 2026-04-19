namespace McpWeatherService.Configuration;

public sealed class WeatherProviderOptions
{
    public const string SectionName = "WeatherProvider";

    public string GeocodingBaseUrl { get; set; } = "https://geocoding-api.open-meteo.com";

    public string ForecastBaseUrl { get; set; } = "https://api.open-meteo.com";

    public int RequestTimeoutSeconds { get; set; } = 20;

    public string UserAgent { get; set; } = "mcp-weather-demo/1.0";
}
