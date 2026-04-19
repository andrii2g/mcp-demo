using McpWeatherChatCli.Chat;
using McpWeatherChatCli.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Configuration.AddUserSecrets<Program>(optional: true, reloadOnChange: false);
builder.Configuration.AddEnvironmentVariables();

builder.Services
    .AddOptions<OpenAiOptions>()
    .Bind(builder.Configuration.GetSection(OpenAiOptions.SectionName))
    .PostConfigure(static options =>
    {
        options.ApiKey ??= Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        options.Model = Environment.GetEnvironmentVariable("OPENAI_MODEL") ?? options.Model;
        options.BaseUrl = Environment.GetEnvironmentVariable("OPENAI_BASE_URL") ?? options.BaseUrl;
    })
    .Validate(static options => Uri.IsWellFormedUriString(options.BaseUrl, UriKind.Absolute), "OpenAI BaseUrl must be absolute.")
    .ValidateOnStart();

builder.Services
    .AddOptions<McpOptions>()
    .Bind(builder.Configuration.GetSection(McpOptions.SectionName))
    .PostConfigure(static options =>
    {
        options.ServerUrl = Environment.GetEnvironmentVariable("MCP_SERVER_URL") ?? options.ServerUrl;
    })
    .Validate(static options => Uri.IsWellFormedUriString(options.ServerUrl, UriKind.Absolute), "MCP ServerUrl must be absolute.")
    .ValidateOnStart();

builder.Services.AddSingleton<ConversationState>();
builder.Services.AddSingleton<ResponseRendering>();
builder.Services.AddHttpClient<OpenAiChatClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<OpenAiOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(60);
});
builder.Services.AddSingleton<ChatLoop>();

using var host = builder.Build();

await host.Services.GetRequiredService<ChatLoop>().RunAsync(CancellationToken.None);
