using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

Console.WriteLine("======== Chat Example ========");


OpenAIChatCompletionService chatCompletionService = new(ConfigurationProvider.OpenAI.ChatModelId, ConfigurationProvider.OpenAI.ApiKey);

Console.WriteLine("Chat content:");
Console.WriteLine("------------------------");

var chatHistory = new ChatHistory("You are a C# expert");

// First user message
chatHistory.AddUserMessage("Hi, I want to understand: await foreach");
MessageOutput();

// First bot assistant message
var reply = await chatCompletionService.GetChatMessageContentAsync(chatHistory);
chatHistory.Add(reply);
MessageOutput();

// Second user message
chatHistory.AddUserMessage("Can I use LINQ with IAsyncEnumerable<> ?");
MessageOutput();

// Second bot assistant message
reply = await chatCompletionService.GetChatMessageContentAsync(chatHistory);
chatHistory.Add(reply);
MessageOutput();


void MessageOutput()
{
    var message = chatHistory.Last();

    Console.WriteLine($"{message.Role}: {message.Content}");
    Console.WriteLine("------------------------");
}
