using System.Net;
using System.Text;
using McpWeatherService.Providers.OpenMeteo;
using Microsoft.Extensions.Logging.Abstractions;

namespace McpWeatherService.Tests;

public sealed class OpenMeteoClientMappingTests
{
    [Fact]
    public async Task SearchLocationAsync_Maps_Geocoding_Response()
    {
        var geocodingClient = CreateHttpClient("""
            {
              "results": [
                {
                  "name": "Lviv",
                  "country": "Ukraine",
                  "country_code": "UA",
                  "admin1": "Lviv Oblast",
                  "latitude": 49.84,
                  "longitude": 24.03,
                  "timezone": "Europe/Kyiv"
                }
              ]
            }
            """);

        var forecastClient = CreateHttpClient("""{ "current": { "time": "2026-04-19T08:00:00Z" } }""");
        var client = new OpenMeteoClient(geocodingClient, forecastClient, NullLogger<OpenMeteoClient>.Instance);

        var location = await client.SearchLocationAsync("Lviv", CancellationToken.None);

        Assert.NotNull(location);
        Assert.Equal("Lviv, Lviv Oblast, Ukraine", location.DisplayName);
        Assert.Equal("UA", location.CountryCode);
    }

    [Fact]
    public async Task GetCurrentWeatherAsync_Maps_Current_Weather()
    {
        var geocodingClient = CreateHttpClient("""{ "results": [] }""");
        var forecastClient = CreateHttpClient("""
            {
              "current": {
                "time": "2026-04-19T08:00:00Z",
                "temperature_2m": 12.4,
                "wind_speed_10m": 8.1,
                "wind_direction_10m": 270,
                "weather_code": 3
              }
            }
            """);

        var client = new OpenMeteoClient(geocodingClient, forecastClient, NullLogger<OpenMeteoClient>.Instance);
        var result = await client.GetCurrentWeatherAsync(49.84, 24.03, CancellationToken.None);

        Assert.NotNull(result.Current);
        Assert.Equal(12.4m, result.Current.Temperature2M);
        Assert.Equal(3, result.Current.WeatherCode);
    }

    private static HttpClient CreateHttpClient(string json)
    {
        return new HttpClient(new FakeHttpMessageHandler(json))
        {
            BaseAddress = new Uri("https://example.test/")
        };
    }

    private sealed class FakeHttpMessageHandler(string json) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
        }
    }
}
