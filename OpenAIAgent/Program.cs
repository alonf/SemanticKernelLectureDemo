using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Experimental.Agents;
using Microsoft.SemanticKernel.Plugins.Core;
using Microsoft.SemanticKernel.Plugins.OpenApi;
using Microsoft.SemanticKernel.Plugins.Web;
using Microsoft.SemanticKernel.Plugins.Web.Bing;

Console.WriteLine("======== Agent + Plugin example ========");


var kernel = Kernel.CreateBuilder()
    .AddOpenAIChatCompletion(
        modelId: ConfigurationProvider.OpenAI.ChatModelId,
        apiKey: ConfigurationProvider.OpenAI.ApiKey)
    .Build();

#pragma warning disable SKEXP0054, SKEXP0042, SKEXP0101, SKEXP0050
kernel.ImportPluginFromType<TimePlugin>();
kernel.ImportPluginFromType<MathPlugin>();

var bingConnector = new BingConnector(ConfigurationProvider.Bing.ApiKey);
var bing = new WebSearchEnginePlugin(bingConnector);
var bingPlugin = kernel.ImportPluginFromObject(bing, "bing");


var agent =
#pragma warning disable 
    await new Microsoft.SemanticKernel.Experimental.Agents.AgentBuilder()
        .WithOpenAIChatCompletion(ConfigurationProvider.OpenAI.ChatModelId, ConfigurationProvider.OpenAI.ApiKey)
        .FromTemplate(GetAgentDefinition())
        .WithPlugin(bingPlugin)
        .BuildAsync();


#pragma warning restore SKEXP0054, SKEXP0042, SKEXP0101, SKEXP0050



Console.WriteLine("Chat content:");
Console.WriteLine("------------------------");

var chatHistory = new ChatHistory("I am a currency exchange expert");

while (true)
{
    Console.Write("You: ");
    var userMessage = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(userMessage))
    {
        continue;
    }

    if (userMessage.ToLower() == "end")
    {
        break;
    }

    // Add user message to chat history
    chatHistory.AddUserMessage(userMessage);

    // Process the message with the agent
    try
    {
#pragma warning disable SKEXP0101
        var result = await agent.AsPlugin().InvokeAsync(userMessage);
#pragma warning restore SKEXP0101

#pragma warning disable SKEXP0101
        await foreach (IChatMessage message in agent.InvokeAsync(userMessage))
        {
            Console.WriteLine($"[{message.Id}]");
            Console.WriteLine($"# {message.Role}: {message.Content}");
            chatHistory.AddMessage(AuthorRole.Assistant, message.Content);
        }
#pragma warning restore SKEXP0101

    }
    catch (Exception e)
    {
        Console.WriteLine($"There was an error: {e.Message}. Try again");
    }

}

// Clean-up resources

#pragma warning disable SKEXP0101
await agent.DeleteAsync();
#pragma warning restore SKEXP0101

string GetAgentDefinition()
{
    return @"
    name: CurrencyExchangeInformationAgent
    description: Know how to convert currencies. Use Bing plugin for current information. Answer with ASCII text tables.
";
}

