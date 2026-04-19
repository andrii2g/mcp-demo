# MCP Notes

## What MCP means in this repo

Model Context Protocol is the boundary between the model-facing client and the weather capability exposed by the service.

In this repository:

- the ASP.NET Core app is the MCP server
- `get_weather` is the only MCP tool
- the CLI never calls Open-Meteo directly
- the CLI tells the Responses API where the remote MCP server lives

## Why remote MCP

This sample is about the real networked shape:

- service and client are separate processes
- the model can discover the tool from the server
- the integration path matches how remote tools are configured in the Responses API

For an educational sample, this is more useful than hiding everything in one local process.

## Service endpoint

The service exposes remote MCP at:

```text
http://localhost:5048/mcp
```

The implementation uses the official MCP C# SDK ASP.NET Core integration and maps the endpoint with:

```csharp
builder.Services.AddMcpServer()
    .WithHttpTransport(options => options.Stateless = true)
    .WithToolsFromAssembly();

app.MapMcp("/mcp");
```

Stateless Streamable HTTP is used because this server does not need server-to-client requests.

## Tool shape

The tool is declared with:

- name `get_weather`
- optional `location`
- optional `city`
- optional `country`

The tool returns both:

- summary text for natural model consumption
- structured JSON for predictable machine-readable use
