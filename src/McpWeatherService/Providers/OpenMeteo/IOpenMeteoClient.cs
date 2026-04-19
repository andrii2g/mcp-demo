using McpWeatherService.Application.Contracts;

namespace McpWeatherService.Providers.OpenMeteo;

public interface IOpenMeteoClient
{
    Task<LocationResolution?> SearchLocationAsync(string searchText, CancellationToken cancellationToken);

    Task<OpenMeteoForecastResponse> GetCurrentWeatherAsync(double latitude, double longitude, CancellationToken cancellationToken);
}
