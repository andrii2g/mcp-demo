using McpWeatherService.Application.Services;
using McpWeatherService.Configuration;
using McpWeatherService.Formatting;
using McpWeatherService.Providers.OpenMeteo;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;

namespace McpWeatherService.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWeatherServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<WeatherProviderOptions>()
            .Bind(configuration.GetSection(WeatherProviderOptions.SectionName))
            .Validate(static options => Uri.IsWellFormedUriString(options.GeocodingBaseUrl, UriKind.Absolute), "GeocodingBaseUrl must be absolute.")
            .Validate(static options => Uri.IsWellFormedUriString(options.ForecastBaseUrl, UriKind.Absolute), "ForecastBaseUrl must be absolute.")
            .ValidateOnStart();

        services.AddSingleton<WeatherResponseFormatter>();
        services.AddScoped<IWeatherService, WeatherService>();

        services.AddHttpClient("open-meteo-geocoding", static (serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<WeatherProviderOptions>>().Value;
            client.BaseAddress = new Uri(options.GeocodingBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.RequestTimeoutSeconds);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);
        });

        services.AddHttpClient("open-meteo-forecast", static (serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<WeatherProviderOptions>>().Value;
            client.BaseAddress = new Uri(options.ForecastBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.RequestTimeoutSeconds);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);
        });

        services.AddScoped<IOpenMeteoClient>(static serviceProvider =>
        {
            var factory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            var logger = serviceProvider.GetRequiredService<ILogger<OpenMeteoClient>>();

            return new OpenMeteoClient(
                factory.CreateClient("open-meteo-geocoding"),
                factory.CreateClient("open-meteo-forecast"),
                logger);
        });

        services.AddMcpServer()
            .WithHttpTransport(options =>
            {
                options.Stateless = true;
            })
            .WithToolsFromAssembly();

        return services;
    }
}
