using McpWeatherService.Application.Contracts;
using McpWeatherService.Application.Services;
using McpWeatherService.Providers.OpenMeteo;
using Microsoft.Extensions.Logging.Abstractions;

namespace McpWeatherService.Tests;

public sealed class WeatherServiceTests
{
    [Fact]
    public async Task Returns_NotFound_When_Geocoder_Has_No_Result()
    {
        var client = new FakeOpenMeteoClient();
        var service = new WeatherService(client, NullLogger<WeatherService>.Instance);

        var result = await service.GetCurrentWeatherAsync(WeatherQuery.Normalize("Foobarville", null, null), CancellationToken.None);

        Assert.False(result.Success);
        Assert.True(result.NotFound);
        Assert.Equal("LocationNotFound", result.ErrorCode);
    }

    [Fact]
    public async Task Returns_Validation_Error_For_Contradictory_Input()
    {
        var client = new FakeOpenMeteoClient();
        var service = new WeatherService(client, NullLogger<WeatherService>.Instance);
        var result = await service.GetCurrentWeatherAsync(WeatherQuery.Normalize("Lviv", "Lviv", "Ukraine"), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("ValidationError", result.ErrorCode);
    }

    [Fact]
    public async Task Maps_Successful_Response()
    {
        var client = new FakeOpenMeteoClient
        {
            SearchResult = new LocationResolution
            {
                DisplayName = "Lviv, Lviv Oblast, Ukraine",
                City = "Lviv",
                Country = "Ukraine",
                CountryCode = "UA",
                Latitude = 49.84,
                Longitude = 24.03,
                TimeZone = "Europe/Kyiv"
            },
            ForecastResponse = new OpenMeteoForecastResponse
            {
                Current = new OpenMeteoCurrentWeather
                {
                    Time = "2026-04-19T08:00:00Z",
                    Temperature2M = 12.4m,
                    WindSpeed10M = 8.1m,
                    WindDirection10M = 270,
                    WeatherCode = 3
                }
            }
        };

        var service = new WeatherService(client, NullLogger<WeatherService>.Instance);
        var result = await service.GetCurrentWeatherAsync(WeatherQuery.Normalize("Lviv", null, null), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("Overcast", result.ConditionText);
        Assert.Equal(12.4m, result.TemperatureC);
        Assert.Equal("Lviv, Lviv Oblast, Ukraine", result.Location?.DisplayName);
    }

    private sealed class FakeOpenMeteoClient : IOpenMeteoClient
    {
        public LocationResolution? SearchResult { get; init; }

        public OpenMeteoForecastResponse ForecastResponse { get; init; } = new();

        public Task<OpenMeteoForecastResponse> GetCurrentWeatherAsync(double latitude, double longitude, CancellationToken cancellationToken) =>
            Task.FromResult(ForecastResponse);

        public Task<LocationResolution?> SearchLocationAsync(string searchText, CancellationToken cancellationToken) =>
            Task.FromResult(SearchResult);
    }
}
