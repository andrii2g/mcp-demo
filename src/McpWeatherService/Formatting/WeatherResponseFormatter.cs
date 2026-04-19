using System.Globalization;
using McpWeatherService.Application.Contracts;

namespace McpWeatherService.Formatting;

public sealed class WeatherResponseFormatter
{
    public string FormatSummary(WeatherResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.Success && result.Location is not null)
        {
            var observedAt = result.ObservedAtUtc?.ToString("O", CultureInfo.InvariantCulture) ?? "unknown time";
            var temperature = result.TemperatureC is null ? "unknown temperature" : $"{result.TemperatureC:0.0}°C";
            var wind = result.WindSpeedKmh is null ? "wind unknown" : $"wind {result.WindSpeedKmh:0.0} km/h";
            var condition = string.IsNullOrWhiteSpace(result.ConditionText)
                ? result.WeatherCode is null ? null : $"weather code {result.WeatherCode}"
                : result.ConditionText;

            var conditionPart = string.IsNullOrWhiteSpace(condition) ? string.Empty : $", {condition}";
            return $"Current weather for {result.Location.DisplayName}: {temperature}, {wind}{conditionPart}, observed at {observedAt}.";
        }

        if (result.NotFound)
        {
            return result.Message ?? "Location not found.";
        }

        return result.Message ?? "Weather lookup failed.";
    }
}
