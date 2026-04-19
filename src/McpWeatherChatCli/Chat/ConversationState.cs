using McpWeatherChatCli.Models;

namespace McpWeatherChatCli.Chat;

public sealed class ConversationState
{
    private readonly List<CliMessage> _messages = [];

    public IReadOnlyList<CliMessage> Messages => _messages;

    public void AddUserMessage(string text) => _messages.Add(new CliMessage { Role = "user", Text = text });

    public void AddAssistantMessage(string text) => _messages.Add(new CliMessage { Role = "assistant", Text = text });
}
