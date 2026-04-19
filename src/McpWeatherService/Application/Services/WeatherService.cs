using McpWeatherService.Application.Contracts;
using McpWeatherService.Providers.OpenMeteo;
using Microsoft.Extensions.Logging;

namespace McpWeatherService.Application.Services;

public sealed class WeatherService(
    IOpenMeteoClient openMeteoClient,
    ILogger<WeatherService> logger) : IWeatherService
{
    private const string ProviderName = "open-meteo";

    public async Task<WeatherResult> GetCurrentWeatherAsync(WeatherQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (!query.IsValid(out var validationMessage))
        {
            return new WeatherResult
            {
                Provider = ProviderName,
                ErrorCode = "ValidationError",
                Message = validationMessage
            };
        }

        var searchText = query.BuildSearchText();
        logger.LogInformation("Resolving weather request for {SearchText}", searchText);

        try
        {
            var location = await openMeteoClient.SearchLocationAsync(searchText, cancellationToken);
            if (location is null)
            {
                logger.LogInformation("Location not found for {SearchText}", searchText);
                return new WeatherResult
                {
                    Provider = ProviderName,
                    NotFound = true,
                    ErrorCode = "LocationNotFound",
                    Message = $"No weather location matched '{searchText}'."
                };
            }

            var forecast = await openMeteoClient.GetCurrentWeatherAsync(location.Latitude, location.Longitude, cancellationToken);
            if (forecast.Current is null)
            {
                logger.LogWarning("Weather response missing current payload for {SearchText}", searchText);
                return new WeatherResult
                {
                    Provider = ProviderName,
                    ErrorCode = "ProviderResponseInvalid",
                    Message = "Weather provider returned an incomplete response.",
                    Location = location
                };
            }

            return new WeatherResult
            {
                Success = true,
                Provider = ProviderName,
                Location = location,
                TemperatureC = forecast.Current.Temperature2M,
                WindSpeedKmh = forecast.Current.WindSpeed10M,
                WindDirectionDegrees = forecast.Current.WindDirection10M,
                WeatherCode = forecast.Current.WeatherCode,
                ConditionText = WeatherCodeInterpreter.ToConditionText(forecast.Current.WeatherCode),
                ObservedAtUtc = ParseObservedAt(forecast.Current.Time)
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "Provider HTTP failure for {SearchText}", searchText);
            return new WeatherResult
            {
                Provider = ProviderName,
                ErrorCode = "ProviderUnavailable",
                Message = "Weather provider request failed."
            };
        }
        catch (InvalidOperationException exception)
        {
            logger.LogError(exception, "Provider payload invalid for {SearchText}", searchText);
            return new WeatherResult
            {
                Provider = ProviderName,
                ErrorCode = "ProviderResponseInvalid",
                Message = "Weather provider returned an invalid payload."
            };
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unexpected weather error for {SearchText}", searchText);
            return new WeatherResult
            {
                Provider = ProviderName,
                ErrorCode = "UnexpectedError",
                Message = "Unexpected weather error."
            };
        }
    }

    private static DateTimeOffset? ParseObservedAt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return DateTimeOffset.TryParse(value, out var parsed)
            ? parsed.ToUniversalTime()
            : null;
    }
}
