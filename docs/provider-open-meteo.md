# Open-Meteo Provider Notes

## Endpoints used

This demo uses two public Open-Meteo endpoints:

- geocoding: `https://geocoding-api.open-meteo.com/v1/search`
- forecast: `https://api.open-meteo.com/v1/forecast`

## Fields used

Geocoding:

- `name`
- `country`
- `country_code`
- `admin1`
- `latitude`
- `longitude`
- `timezone`

Forecast current weather:

- `time`
- `temperature_2m`
- `wind_speed_10m`
- `wind_direction_10m`
- `weather_code`

## Mapping choices

- only the first geocoding result is used in the initial demo
- provider DTOs stay in `Providers/OpenMeteo`
- application contracts stay provider-agnostic
- weather codes are interpreted into a small readable condition set

## Known limitations

- no ambiguity-resolution flow beyond first result wins
- no unit conversion layer yet
- no caching or rate-limit handling
- no provider failover

## Replacement strategy

`WeatherService` only depends on `IOpenMeteoClient`. If the repo later grows into a multi-provider sample, the service can move to a more generic weather provider interface without changing the tool contract.
