using System.Text.Json;
using McpWeatherChatCli.Chat;

namespace McpWeatherChatCli.Tests;

public sealed class ResponseRenderingTests
{
    [Fact]
    public void Uses_OutputText_When_Present()
    {
        var rendering = new ResponseRendering();
        using var document = JsonDocument.Parse("""{ "output_text": "Weather is mild." }""");

        var text = rendering.ExtractAssistantText(document.RootElement);

        Assert.Equal("Weather is mild.", text);
    }

    [Fact]
    public void Falls_Back_To_Output_Content()
    {
        var rendering = new ResponseRendering();
        using var document = JsonDocument.Parse("""
            {
              "output": [
                {
                  "content": [
                    { "text": "Line one." },
                    { "text": "Line two." }
                  ]
                }
              ]
            }
            """);

        var text = rendering.ExtractAssistantText(document.RootElement);

        Assert.Contains("Line one.", text);
        Assert.Contains("Line two.", text);
    }

    [Fact]
    public void Returns_Fallback_When_No_Text_Is_Present()
    {
        var rendering = new ResponseRendering();
        using var document = JsonDocument.Parse("""{ "output": [] }""");

        var text = rendering.ExtractAssistantText(document.RootElement);

        Assert.Equal("Assistant returned no text output.", text);
    }
}
