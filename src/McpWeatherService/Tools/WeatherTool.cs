using System.ComponentModel;
using System.Text.Json;
using McpWeatherService.Application.Contracts;
using McpWeatherService.Application.Services;
using McpWeatherService.Formatting;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Shared.Json;

namespace McpWeatherService.Tools;

[McpServerToolType]
public sealed class WeatherTool(
    IWeatherService weatherService,
    WeatherResponseFormatter formatter)
{
    [McpServerTool(
        Name = "get_weather",
        ReadOnly = true,
        Idempotent = true,
        OpenWorld = true,
        UseStructuredContent = true,
        OutputSchemaType = typeof(WeatherToolPayload))]
    [Description("Get the current weather for a location using Open-Meteo geocoding and forecast data.")]
    public async Task<CallToolResult> GetWeatherAsync(
        [Description("Free-form location such as 'Lviv' or 'Lviv, Ukraine'.")] string? location = null,
        [Description("City name when location is not provided.")] string? city = null,
        [Description("Optional country name or code when city is used.")] string? country = null,
        CancellationToken cancellationToken = default)
    {
        var query = WeatherQuery.Normalize(location, city, country);
        var result = await weatherService.GetCurrentWeatherAsync(query, cancellationToken);
        var summary = formatter.FormatSummary(result);
        var payload = WeatherToolPayload.From(result, summary);

        return new CallToolResult
        {
            IsError = !result.Success && !result.NotFound,
            StructuredContent = JsonSerializer.SerializeToElement(payload, JsonDefaults.Options),
            Content =
            [
                new TextContentBlock
                {
                    Text = summary
                }
            ]
        };
    }
}

public sealed class WeatherToolPayload
{
    public required string Provider { get; init; }

    public required bool Success { get; init; }

    public required bool NotFound { get; init; }

    public required string Summary { get; init; }

    public string? ErrorCode { get; init; }

    public string? Message { get; init; }

    public WeatherToolLocation? Location { get; init; }

    public WeatherToolCurrent? Current { get; init; }

    public static WeatherToolPayload From(WeatherResult result, string summary)
    {
        return new WeatherToolPayload
        {
            Provider = result.Provider,
            Success = result.Success,
            NotFound = result.NotFound,
            Summary = summary,
            ErrorCode = result.ErrorCode,
            Message = result.Message,
            Location = result.Location is null ? null : new WeatherToolLocation
            {
                DisplayName = result.Location.DisplayName,
                Country = result.Location.Country,
                CountryCode = result.Location.CountryCode,
                Latitude = result.Location.Latitude,
                Longitude = result.Location.Longitude,
                TimeZone = result.Location.TimeZone
            },
            Current = !result.Success ? null : new WeatherToolCurrent
            {
                TemperatureC = result.TemperatureC,
                WindSpeedKmh = result.WindSpeedKmh,
                WindDirectionDegrees = result.WindDirectionDegrees,
                WeatherCode = result.WeatherCode,
                ConditionText = result.ConditionText,
                ObservedAtUtc = result.ObservedAtUtc
            }
        };
    }
}

public sealed class WeatherToolLocation
{
    public required string DisplayName { get; init; }

    public string? Country { get; init; }

    public string? CountryCode { get; init; }

    public double Latitude { get; init; }

    public double Longitude { get; init; }

    public string? TimeZone { get; init; }
}

public sealed class WeatherToolCurrent
{
    public decimal? TemperatureC { get; init; }

    public decimal? WindSpeedKmh { get; init; }

    public int? WindDirectionDegrees { get; init; }

    public int? WeatherCode { get; init; }

    public string? ConditionText { get; init; }

    public DateTimeOffset? ObservedAtUtc { get; init; }
}
