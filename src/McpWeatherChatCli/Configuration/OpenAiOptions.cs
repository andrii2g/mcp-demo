namespace McpWeatherChatCli.Configuration;

public sealed class OpenAiOptions
{
    public const string SectionName = "OpenAI";

    public string BaseUrl { get; set; } = "https://api.openai.com/v1/";

    public string Model { get; set; } = "gpt-5.4";

    public string? ApiKey { get; set; }
}
