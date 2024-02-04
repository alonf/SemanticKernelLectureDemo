using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

Console.WriteLine("======== Streaming Chat Example ========");


OpenAIChatCompletionService chatCompletionService = new(ConfigurationProvider.OpenAI.ChatModelId, ConfigurationProvider.OpenAI.ApiKey);

Console.WriteLine("Chat content:");
Console.WriteLine("------------------------");

var chatHistory = new ChatHistory("You are a C# expert");

// First user message
chatHistory.AddUserMessage("Hi, I want to understand: await foreach");
MessageOutput();

var reply = await chatCompletionService.GetChatMessageContentAsync(chatHistory);
chatHistory.Add(reply);
MessageOutput();

// First bot assistant message
//await StreamMessageOutputAsync();


// Second user message
chatHistory.AddUserMessage("Can I use LINQ with IAsyncEnumerable<> ?");
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

    await foreach (var chatUpdate in chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory))
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