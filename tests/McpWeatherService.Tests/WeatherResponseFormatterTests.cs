using McpWeatherService.Application.Contracts;
using McpWeatherService.Formatting;

namespace McpWeatherService.Tests;

public sealed class WeatherResponseFormatterTests
{
    [Fact]
    public void Formats_Success_Response()
    {
        var formatter = new WeatherResponseFormatter();
        var result = new WeatherResult
        {
            Success = true,
            Provider = "open-meteo",
            Location = new LocationResolution
            {
                DisplayName = "Lviv, Ukraine",
                Latitude = 49.84,
                Longitude = 24.03
            },
            TemperatureC = 12.4m,
            WindSpeedKmh = 8.1m,
            ConditionText = "Overcast",
            ObservedAtUtc = DateTimeOffset.Parse("2026-04-19T08:00:00Z")
        };

        var summary = formatter.FormatSummary(result);

        Assert.Contains("Current weather for Lviv, Ukraine", summary);
        Assert.Contains("12.4°C", summary);
        Assert.Contains("Overcast", summary);
    }

    [Fact]
    public void Formats_NotFound_Response()
    {
        var formatter = new WeatherResponseFormatter();
        var summary = formatter.FormatSummary(new WeatherResult
        {
            Provider = "open-meteo",
            NotFound = true,
            Message = "No weather location matched 'Foobarville'."
        });

        Assert.Equal("No weather location matched 'Foobarville'.", summary);
    }
}
