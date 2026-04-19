# mcp-demo

A small .NET 10 example of:

- a remote MCP weather service
- a CLI chatbot using the OpenAI Responses API
- a public MCP endpoint exposed through `ngrok`

## Projects

- `src/McpWeatherService`: ASP.NET Core MCP server with one tool, `get_weather`
- `src/McpWeatherChatCli`: console chat client that registers the MCP server as a remote MCP tool

## Prerequisites

- .NET 10 SDK
- `OPENAI_API_KEY`
- `ngrok`

## 1. Run the service locally

```powershell
dotnet run --project src/McpWeatherService
```

Local health check:

```powershell
curl.exe http://localhost:5048/health
```

Expected:

```json
{"status":"ok"}
```

## 2. Expose the service with ngrok

Start a tunnel to the local service:

```powershell
ngrok http 5048
```

ngrok will give you a public HTTPS host, for example:

```text
https://your-ngrok-host.ngrok-free.app
```

Use that host for the MCP endpoint:

```text
https://your-ngrok-host.ngrok-free.app/mcp
```

Public health check:

```powershell
curl.exe https://your-ngrok-host.ngrok-free.app/health
```

## 3. Configure the CLI

Set your OpenAI key:

```powershell
$env:OPENAI_API_KEY = "..."
```

Set the public MCP URL:

```powershell
$env:MCP_SERVER_URL = "https://your-ngrok-host.ngrok-free.app/mcp"
```

Optional:

```powershell
$env:OPENAI_MODEL = "gpt-5.4"
```

## 4. Run the CLI

```powershell
dotnet run --project src/McpWeatherChatCli
```

Example prompts:

- `What is the weather in Lviv?`
- `How is the weather in Berlin, Germany right now?`
- `What is the weather in a city that probably does not exist: Foobarville?`

## Visual Studio / User Secrets

If you run the CLI from Visual Studio, set secrets for the `McpWeatherChatCli` project.

Example:

```json
{
  "OpenAI": {
    "ApiKey": "..."
  },
  "Mcp": {
    "ServerUrl": "https://your-ngrok-host.ngrok-free.app/mcp"
  }
}
```

Do not use `http://localhost:5048/mcp` for `Mcp:ServerUrl` with the Responses API remote MCP flow. OpenAI must be able to reach the MCP server from the public internet.

## Test

```powershell
dotnet test mcp-weather-demo.sln
```

## Notes

- The service listens locally on `http://localhost:5048`.
- The CLI must use the public HTTPS MCP URL, not localhost.
- If you restart ngrok, the host may change. Update `MCP_SERVER_URL` or your user secrets when that happens.
