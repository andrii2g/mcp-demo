using System.Text.Json;

namespace McpWeatherChatCli.Chat;

public sealed class ResponseRendering
{
    public string ExtractAssistantText(JsonElement response)
    {
        if (response.TryGetProperty("output_text", out var outputText) && outputText.ValueKind == JsonValueKind.String)
        {
            var text = outputText.GetString();
            if (!string.IsNullOrWhiteSpace(text))
            {
                return text.Trim();
            }
        }

        if (!response.TryGetProperty("output", out var output) || output.ValueKind != JsonValueKind.Array)
        {
            return "Assistant returned no text output.";
        }

        var chunks = new List<string>();

        foreach (var item in output.EnumerateArray())
        {
            if (!item.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var contentItem in content.EnumerateArray())
            {
                if (!contentItem.TryGetProperty("text", out var textElement) || textElement.ValueKind != JsonValueKind.String)
                {
                    continue;
                }

                var text = textElement.GetString();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    chunks.Add(text.Trim());
                }
            }
        }

        return chunks.Count == 0
            ? "Assistant returned no text output."
            : string.Join(Environment.NewLine + Environment.NewLine, chunks);
    }
}
