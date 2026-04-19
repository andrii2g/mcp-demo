using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using McpWeatherChatCli.Configuration;
using McpWeatherChatCli.Models;
using Microsoft.Extensions.Options;
using Shared.Json;

namespace McpWeatherChatCli.Chat;

public sealed class OpenAiChatClient(
    HttpClient httpClient,
    IOptions<OpenAiOptions> openAiOptions,
    IOptions<McpOptions> mcpOptions)
{
    public async Task<JsonDocument> CreateResponseAsync(IReadOnlyList<CliMessage> messages, CancellationToken cancellationToken)
    {
        var options = openAiOptions.Value;
        var apiKey = ResolveApiKey(options);

        var payload = new
        {
            model = options.Model,
            input = messages.Select(static message => new
            {
                role = message.Role,
                content = new[]
                {
                    new
                    {
                        type = "input_text",
                        text = message.Text
                    }
                }
            }),
            tools = new object[]
            {
                new
                {
                    type = "mcp",
                    server_label = "weather-service",
                    server_description = "Current weather lookup tool powered by Open-Meteo.",
                    server_url = NormalizeServerUrl(mcpOptions.Value.ServerUrl),
                    allowed_tools = new[] { "get_weather" },
                    require_approval = "never"
                }
            }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "responses");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(payload, JsonDefaults.Options), Encoding.UTF8, "application/json");

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"OpenAI request failed: {(int)response.StatusCode} {response.ReasonPhrase}. {content}");
        }

        return JsonDocument.Parse(content);
    }

    private static string ResolveApiKey(OpenAiOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.ApiKey))
        {
            return options.ApiKey;
        }

        throw new InvalidOperationException("OpenAI API key is missing. Set OPENAI_API_KEY and retry.");
    }

    private static string NormalizeServerUrl(string serverUrl) =>
        serverUrl.EndsWith("/mcp", StringComparison.OrdinalIgnoreCase)
            ? serverUrl
            : $"{serverUrl.TrimEnd('/')}/mcp";
}
