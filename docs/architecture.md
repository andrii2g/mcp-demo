# Architecture

## Service layers

`src/McpWeatherService` is intentionally split into a few small layers.

### Transport

- `Program.cs`
- `Endpoints/HealthEndpoints.cs`
- `Tools/WeatherTool.cs`
- `Infrastructure/ServiceCollectionExtensions.cs`

Responsibilities:

- boot the ASP.NET Core host
- wire the MCP server with `AddMcpServer().WithHttpTransport(...).WithToolsFromAssembly()`
- expose `GET /health`
- expose `/mcp`

### Application

- `Application/Contracts/*`
- `Application/Services/*`

Responsibilities:

- normalize input
- validate supported argument combinations
- orchestrate geocoding then weather fetch
- translate provider failures into stable result shapes

### Provider

- `Providers/OpenMeteo/*`

Responsibilities:

- call Open-Meteo endpoints
- keep provider JSON DTOs isolated from app contracts
- map the first geocoding result into `LocationResolution`

### Formatting

- `Formatting/WeatherResponseFormatter.cs`

Responsibilities:

- generate the concise human-readable summary returned to the model and useful in debugging

## Request flow

1. The CLI sends a Responses API request with a remote MCP tool entry.
2. OpenAI imports the tool list from `http://localhost:5048/mcp`.
3. The model decides whether to call `get_weather`.
4. The tool constructs a `WeatherQuery`.
5. `WeatherService` resolves the location and current weather via `IOpenMeteoClient`.
6. The tool returns:
   - text content: summary string
   - structured content: normalized JSON payload

## Why this shape

- It is small enough to read quickly.
- It avoids coupling tool handlers directly to raw HTTP.
- It leaves space for future tools, auth, metrics, caching, and richer provider behavior without a rewrite.
