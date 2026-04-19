namespace McpWeatherChatCli.Models;

public sealed class CliMessage
{
    public required string Role { get; init; }

    public required string Text { get; init; }
}
