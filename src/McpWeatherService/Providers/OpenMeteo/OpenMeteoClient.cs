using System.Globalization;
using System.Net.Http.Json;
using McpWeatherService.Application.Contracts;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace McpWeatherService.Providers.OpenMeteo;

public sealed class OpenMeteoClient(
    HttpClient geocodingHttpClient,
    HttpClient forecastHttpClient,
    ILogger<OpenMeteoClient> logger) : IOpenMeteoClient
{
    public async Task<LocationResolution?> SearchLocationAsync(string searchText, CancellationToken cancellationToken)
    {
        var requestUri = QueryHelpers.AddQueryString(
            "v1/search",
            new Dictionary<string, string?>
            {
                ["name"] = searchText,
                ["count"] = "1",
                ["language"] = "en",
                ["format"] = "json"
            });

        logger.LogInformation("Open-Meteo geocoding lookup for {SearchText}", searchText);

        using var response = await geocodingHttpClient.GetAsync(requestUri, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Geocoding request failed with status code {(int)response.StatusCode}.");
        }

        var payload = await response.Content.ReadFromJsonAsync<OpenMeteoGeocodingResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Could not parse geocoding response.");

        var result = payload.Results?.FirstOrDefault();
        if (result is null)
        {
            return null;
        }

        return new LocationResolution
        {
            DisplayName = BuildDisplayName(result),
            City = result.Name,
            Country = result.Country,
            CountryCode = result.CountryCode,
            Latitude = result.Latitude,
            Longitude = result.Longitude,
            TimeZone = result.Timezone
        };
    }

    public async Task<OpenMeteoForecastResponse> GetCurrentWeatherAsync(double latitude, double longitude, CancellationToken cancellationToken)
    {
        var requestUri = QueryHelpers.AddQueryString(
            "v1/forecast",
            new Dictionary<string, string?>
            {
                ["latitude"] = latitude.ToString(CultureInfo.InvariantCulture),
                ["longitude"] = longitude.ToString(CultureInfo.InvariantCulture),
                ["current"] = "temperature_2m,wind_speed_10m,wind_direction_10m,weather_code",
                ["timezone"] = "UTC",
                ["forecast_days"] = "1"
            });

        logger.LogInformation("Open-Meteo forecast lookup for {Latitude}, {Longitude}", latitude, longitude);

        using var response = await forecastHttpClient.GetAsync(requestUri, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Forecast request failed with status code {(int)response.StatusCode}.");
        }

        return await response.Content.ReadFromJsonAsync<OpenMeteoForecastResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Could not parse forecast response.");
    }

    private static string BuildDisplayName(OpenMeteoGeocodingResult result)
    {
        var parts = new[]
        {
            result.Name,
            result.Admin1,
            result.Country
        }
        .Where(static part => !string.IsNullOrWhiteSpace(part))
        .Distinct(StringComparer.OrdinalIgnoreCase);

        return string.Join(", ", parts);
    }
}
