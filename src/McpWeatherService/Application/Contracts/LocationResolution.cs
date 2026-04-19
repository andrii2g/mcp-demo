namespace McpWeatherService.Application.Contracts;

public sealed class LocationResolution
{
    public required string DisplayName { get; init; }

    public string? City { get; init; }

    public string? Country { get; init; }

    public string? CountryCode { get; init; }

    public double Latitude { get; init; }

    public double Longitude { get; init; }

    public string? TimeZone { get; init; }
}
