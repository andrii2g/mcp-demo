# PLAN.md - mcp-demo

## Purpose

This repository demonstrates the smallest still-useful end-to-end implementation of:

1. a remote MCP service in C# on .NET 10
2. a single MCP tool named `get_weather`
3. a real HTTP integration to Open-Meteo
4. a separate CLI chatbot using the OpenAI Responses API
5. remote MCP tool registration from the CLI to the service

The repository is a teaching/demo repo first. Clarity and easy modification are valued over completeness.

## Product framing

Repository goal:

> How little code do I need to create a real MCP server and connect it to an LLM-powered client?

Non-goals:

- production weather application
- generic agent platform
- full authentication solution
- UI-heavy product
- performance benchmark

Primary demo story:

- run the MCP weather service locally
- run the CLI locally
- ask `what is the weather in Lviv?`
- let the model discover and call `get_weather`
- return structured weather data plus human-readable summary text

## Architecture

```text
User terminal
    |
    v
Chat CLI (.NET 10)
    |
    v
OpenAI Responses API request with remote MCP tool config
    |
    v
MCP Weather Service (ASP.NET Core + MCP C# SDK)
    |
    v
Open-Meteo geocoding + weather APIs
```

Key constraints:

- keep service transport, application, and provider layers separate
- keep DTOs explicit and small
- keep CLI weather-free; weather behavior lives behind the MCP tool path
- keep provider integration replaceable

## Functional requirements

Service:

- `GET /health`
- remote MCP endpoint at `/mcp`
- one tool: `get_weather`

`get_weather` input:

- `location`
- or `city` plus optional `country`

Valid examples:

- `{ "location": "Lviv" }`
- `{ "location": "Lviv, Ukraine" }`
- `{ "city": "Berlin", "country": "DE" }`

Invalid examples:

- empty request
- whitespace-only values
- contradictory `location` plus `city`/`country`

Tool behavior:

1. normalize input
2. geocode via Open-Meteo
3. return structured not-found when no match exists
4. fetch current weather for resolved coordinates
5. return normalized result with summary text and structured fields

CLI requirements:

- read user input
- call Responses API
- register the remote MCP server in `tools`
- render assistant text
- keep conversation continuity
- support `exit` and `quit`

## Implementation status

Implemented in this repo:

- solution/bootstrap on .NET 10
- service with `/health`
- Open-Meteo geocoding + forecast integration
- application service and formatter
- MCP `get_weather` tool with structured content plus text content
- CLI with Responses API request construction and remote MCP tool config
- unit tests for service, mapping, formatter, and CLI rendering
- docs and sample files

## Acceptance checklist

- [x] Solution builds on .NET 10
- [x] MCP Weather Service starts locally
- [x] `GET /health` returns `{ "status": "ok" }`
- [x] `get_weather` accepts `location` or `city` + `country`
- [x] tool calls Open-Meteo geocoding and forecast endpoints
- [x] tool returns normalized structured payload and summary text
- [x] CLI starts with `OPENAI_API_KEY`
- [x] CLI registers MCP server as a remote MCP tool via Responses API
- [ ] natural-language weather prompt verified end-to-end against live OpenAI API
- [x] not-found scenario handled gracefully
- [x] unit tests cover core mapping/formatting behavior
- [x] README explains how to run the demo

The remaining unchecked item requires a live API key and network-backed manual run.
