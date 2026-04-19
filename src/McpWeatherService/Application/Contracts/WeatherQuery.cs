using System.Text.RegularExpressions;

namespace McpWeatherService.Application.Contracts;

public sealed class WeatherQuery
{
    private static readonly Regex NonWordPattern = new(@"[\p{L}\p{N}]+", RegexOptions.Compiled);

    public string? Location { get; init; }

    public string? City { get; init; }

    public string? Country { get; init; }

    public bool IsEmpty =>
        string.IsNullOrWhiteSpace(Location) &&
        string.IsNullOrWhiteSpace(City) &&
        string.IsNullOrWhiteSpace(Country);

    public static WeatherQuery Normalize(string? location, string? city, string? country)
    {
        return new WeatherQuery
        {
            Location = NormalizePart(location),
            City = NormalizePart(city),
            Country = NormalizePart(country)
        };
    }

    public bool IsValid(out string? validationMessage)
    {
        if (IsEmpty)
        {
            validationMessage = "Provide either a location or a city, optionally with a country.";
            return false;
        }

        if (!string.IsNullOrWhiteSpace(Location) && (!string.IsNullOrWhiteSpace(City) || !string.IsNullOrWhiteSpace(Country)))
        {
            validationMessage = "Use either location or city/country fields, not both.";
            return false;
        }

        if (!ContainsLettersOrDigits(Location) || !ContainsLettersOrDigits(City))
        {
            validationMessage = "Location and city values must contain letters or digits.";
            return false;
        }

        if (!ContainsLettersOrDigits(Country))
        {
            validationMessage = "Country values must contain letters or digits.";
            return false;
        }

        validationMessage = null;
        return true;
    }

    public string BuildSearchText()
    {
        if (!string.IsNullOrWhiteSpace(Location))
        {
            return Location;
        }

        return string.IsNullOrWhiteSpace(Country)
            ? City ?? string.Empty
            : $"{City}, {Country}";
    }

    private static string? NormalizePart(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        return normalized.Length == 0 ? null : normalized;
    }

    private static bool ContainsLettersOrDigits(string? value) =>
        value is null || NonWordPattern.IsMatch(value);
}
