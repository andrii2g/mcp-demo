using McpWeatherService.Application.Contracts;

namespace McpWeatherService.Application.Services;

public interface IWeatherService
{
    Task<WeatherResult> GetCurrentWeatherAsync(WeatherQuery query, CancellationToken cancellationToken);
}
