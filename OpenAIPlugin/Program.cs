using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.OpenApi;
using Microsoft.SemanticKernel.Plugins.Web;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Microsoft.Extensions.Logging;

Console.WriteLine("======== Plugin example, Make Sure WPT is running ========");

#pragma warning disable SKEXP0054, SKEXP0042, SKEXP0101

var builder = Kernel.CreateBuilder();
//builder.Services.AddLogging(c => c.AddConsole().SetMinimumLevel(LogLevel.Trace));
builder.Services.AddOpenAIChatCompletion(
        modelId: ConfigurationProvider.OpenAI.ChatModelId,
        apiKey: ConfigurationProvider.OpenAI.ApiKey);

var kernel = builder.Build();

var bingConnector = new BingConnector(ConfigurationProvider.Bing.ApiKey);
var bing = new WebSearchEnginePlugin(bingConnector);
var bingPlugin = kernel.ImportPluginFromObject(bing, "bing");

var httpClient = new HttpClient();
httpClient.Timeout = TimeSpan.FromSeconds(600); //todo: move to configuration

var llmModelApiKey = ConfigurationProvider.OpenAI.ApiKey;
var llmModelName = ConfigurationProvider.OpenAI.ChatModelId;

httpClient.DefaultRequestHeaders.Add("LLM-Model-Api-Key", llmModelApiKey);
httpClient.DefaultRequestHeaders.Add("LLM-Model-Name", llmModelName);

var windowsTroubleshootingPlugin = await kernel.ImportPluginFromOpenAIAsync("wpt", new Uri("http://localhost:5000/.well-known/ai-plugin.json"),
     new OpenAIFunctionExecutionParameters
     {
         HttpClient = httpClient
     });

Console.WriteLine("======== Windows Troubleshooting Agent Chat Example ========");

var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

Console.WriteLine("Chat content:");
Console.WriteLine("------------------------");

var chatHistory = new ChatHistory("I am a Windows Troubleshooting Agent. I get request from the user and pass them with all associate information for the WPT plugin to get PC and file information");


//First user message
chatHistory.AddUserMessage(
    """
    Use the following tenant id: 7f8bfcae-90d2-4f2f-8914-0966ffd40786 and pc id: 8e5cb27c-008c-4b79-b211-80b67f10e680 
    You must read at least 30 lines of the content of the file: C:\Dev\SemanticKernelExamples\OpenAIPlugin\Program.cs
    Once you have the file content use Console color sequence to colorize the C# parts such as keywords, constants, use color code similar to Visual Studio.
    I'll reward you with $100 if you do it!
""");

MessageOutput();

// First bot assistant message
await StreamMessageOutputAsync();



// Second user message
chatHistory.AddUserMessage("Provide a URLs that describe the technology that the program uses");
MessageOutput();

// Second bot assistant message
await StreamMessageOutputAsync();


void MessageOutput()
{
    var message = chatHistory.Last();

    Console.WriteLine($"{message.Role}: {message.Content}");
    Console.WriteLine("------------------------");
}


async Task StreamMessageOutputAsync()
{
    bool roleWritten = false;
    string fullMessage = string.Empty;

    OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
    {
        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
        ChatSystemPrompt = "Use Bing plugin to search the web and WPT to get Windows Troubleshooting Platform information for PC and Tenant represented by Guids. When calling the wpt plugin pass all information. Be verbose.",
        Temperature = 0.7,
        TopP = 0.2
        // ... other settings
    };

    await foreach (var chatUpdate in chatCompletionService!.GetStreamingChatMessageContentsAsync(chatHistory,
                       openAIPromptExecutionSettings, kernel))
    {
        if (!roleWritten && chatUpdate.Role.HasValue)
        {
            Console.Write($"{chatUpdate.Role.Value}: {chatUpdate.Content}");
            roleWritten = true;
        }

        if (chatUpdate.Content is { Length: > 0 })
        {
            fullMessage += chatUpdate.Content;
            Console.Write(chatUpdate.Content);
        }
    }

    Console.WriteLine("\n------------------------");
    chatHistory.AddMessage(AuthorRole.Assistant, fullMessage);
}