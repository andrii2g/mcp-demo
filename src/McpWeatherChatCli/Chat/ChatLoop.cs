using McpWeatherChatCli.Configuration;
using Microsoft.Extensions.Options;

namespace McpWeatherChatCli.Chat;

public sealed class ChatLoop(
    OpenAiChatClient openAiChatClient,
    ConversationState conversationState,
    ResponseRendering responseRendering,
    IOptions<OpenAiOptions> openAiOptions,
    IOptions<McpOptions> mcpOptions)
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var serverUrl = NormalizeServerUrl(mcpOptions.Value.ServerUrl);
        EnsureRemoteMcpUrlIsReachable(serverUrl);

        Console.WriteLine("MCP Weather Chat CLI");
        Console.WriteLine($"Model: {openAiOptions.Value.Model}");
        Console.WriteLine($"MCP server: {serverUrl}");
        Console.WriteLine("Type a message, or 'exit' to quit.");
        Console.WriteLine();

        while (!cancellationToken.IsCancellationRequested)
        {
            Console.Write("you> ");
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                continue;
            }

            if (string.Equals(input, "exit", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "quit", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            conversationState.AddUserMessage(input);

            try
            {
                using var response = await openAiChatClient.CreateResponseAsync(conversationState.Messages, cancellationToken);
                var assistantText = responseRendering.ExtractAssistantText(response.RootElement);
                conversationState.AddAssistantMessage(assistantText);

                Console.WriteLine();
                Console.WriteLine($"assistant> {assistantText}");
                Console.WriteLine();
            }
            catch (Exception exception)
            {
                Console.WriteLine();
                Console.WriteLine($"error> {exception.Message}");
                Console.WriteLine();
            }
        }
    }

    private static string NormalizeServerUrl(string serverUrl) =>
        serverUrl.EndsWith("/mcp", StringComparison.OrdinalIgnoreCase)
            ? serverUrl
            : $"{serverUrl.TrimEnd('/')}/mcp";

    private static void EnsureRemoteMcpUrlIsReachable(string serverUrl)
    {
        if (!Uri.TryCreate(serverUrl, UriKind.Absolute, out var uri))
        {
            throw new InvalidOperationException($"MCP server URL is invalid: {serverUrl}");
        }

        if (uri.IsLoopback || IsLocalHostName(uri.Host))
        {
            throw new InvalidOperationException(
                $"MCP_SERVER_URL must be a publicly reachable HTTPS URL for OpenAI remote MCP import. Current value '{serverUrl}' points to localhost. Use a tunnel such as ngrok or cloudflared and set MCP_SERVER_URL to that public /mcp endpoint.");
        }

        if (!string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"MCP_SERVER_URL should use HTTPS for remote MCP import. Current value: '{serverUrl}'.");
        }
    }

    private static bool IsLocalHostName(string host) =>
        string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(host, "127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(host, "::1", StringComparison.OrdinalIgnoreCase);
}
