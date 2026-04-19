namespace McpWeatherService.Application.Contracts;

public sealed class WeatherResult
{
    public bool Success { get; init; }

    public bool NotFound { get; init; }

    public required string Provider { get; init; }

    public string? ErrorCode { get; init; }

    public string? Message { get; init; }

    public LocationResolution? Location { get; init; }

    public decimal? TemperatureC { get; init; }

    public decimal? WindSpeedKmh { get; init; }

    public int? WindDirectionDegrees { get; init; }

    public int? WeatherCode { get; init; }

    public string? ConditionText { get; init; }

    public DateTimeOffset? ObservedAtUtc { get; init; }
}
