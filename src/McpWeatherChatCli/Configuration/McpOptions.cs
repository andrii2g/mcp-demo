namespace McpWeatherChatCli.Configuration;

public sealed class McpOptions
{
    public const string SectionName = "Mcp";

    public string ServerUrl { get; set; } = "http://localhost:5048/mcp";
}
