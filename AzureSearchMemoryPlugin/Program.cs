using Azure;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AzureAISearch;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Plugins.Memory;

//create Azure AI Search memory store
#pragma warning disable SKEXP0003, SKEXP0021, SKEXP0011, SKEXP0052
IMemoryStore memoryStore = new AzureAISearchMemoryStore(ConfigurationProvider.AzureAISearch.Endpoint, ConfigurationProvider.AzureAISearch.ApiKey);
int memoryEntry = 1;

//check if the collection exists
if (await memoryStore.DoesCollectionExistAsync("ExampleCollection"))
{
    Console.WriteLine("Collection ExampleCollection already exists");
    //ask if we want to delete it
    Console.WriteLine("Do you want to delete it? (y/n)");
    var answer = Console.ReadLine();

    if ((answer?.ToLower() ?? "n").StartsWith("y"))
    {
        await memoryStore.DeleteCollectionAsync("ExampleCollection");
    }
    else //show the content of the collection
    {
        Console.WriteLine("Collection content:");

        try
        {

            while (true)
            {
                var memoryRecord = await memoryStore.GetAsync("ExampleCollection", $"info{memoryEntry}");
                if (memoryRecord == null)
                {
                    break;
                }

                Console.WriteLine($"#{memoryEntry}: {memoryRecord.Metadata.Text}");
                memoryEntry++;
            }
        }
        catch (RequestFailedException ex) when(ex.Status == 404)
        {
            Console.WriteLine("No more entries.");
        }
    }
}



var kernel = Kernel.CreateBuilder()
    .AddOpenAIChatCompletion(ConfigurationProvider.OpenAI.ChatModelId, ConfigurationProvider.OpenAI.ApiKey)
    .AddOpenAITextEmbeddingGeneration(ConfigurationProvider.OpenAI.EmbeddingModelId, ConfigurationProvider.OpenAI.ApiKey)
    .Build();

// Create an embedding generator to use for semantic memory.
var embeddingGenerator = new OpenAITextEmbeddingGenerationService(ConfigurationProvider.OpenAI.EmbeddingModelId, ConfigurationProvider.OpenAI.ApiKey);

// The combination of the text embedding generator and the memory store makes up the 'SemanticTextMemory' object used to
// store and retrieve memories.
SemanticTextMemory textMemory = new(memoryStore, embeddingGenerator);

await textMemory.SaveInformationAsync("ExampleCollection", id: "SavedDirectlyInfo", text: "I was saved using textMemory.SaveInformationAsync");

// Import the TextMemoryPlugin into the Kernel 
var memoryPlugin = kernel.ImportPluginFromObject(new TextMemoryPlugin(textMemory));

Console.WriteLine("\nEnter information to store in memory. Enter End to finish:");

while (true)
{
    Console.Write($"#{memoryEntry}: ");
    var text = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(text))
    {
        continue;
    }

    if (text.ToLower() == "end")
    {
        break;
    }

    await kernel.InvokeAsync(memoryPlugin["Save"], new()
    {
        [TextMemoryPlugin.InputParam] = text,
        [TextMemoryPlugin.CollectionParam] = "ExampleCollection",
        [TextMemoryPlugin.KeyParam] = $"info{memoryEntry}"
    });

    //the above is equivalent to:
    //await textMemory.SaveInformationAsync("ExampleCollection", id: $"info{i}", text: text);

    ++memoryEntry;
}

--memoryEntry; //we did not store the last "end" entry

// Retrieve a specific memory with the Kernel
Console.WriteLine("Retrieving Memories through the Kernel with TextMemoryPlugin and the 'Retrieve' function");
var result = await kernel.InvokeAsync(memoryPlugin["Retrieve"], new KernelArguments()
{
    [TextMemoryPlugin.CollectionParam] = "ExampleCollection",
    [TextMemoryPlugin.KeyParam] = $"info{memoryEntry}"
});

Console.WriteLine($"The last stored info{memoryEntry}: {result.GetValue<string>()}");
Console.WriteLine();

Console.WriteLine("Enter instructions for the prompt text to search. Enter End to stop");


//you can search using the TextMemoryPlugin and the 'Search' function: https://github.com/microsoft/semantic-kernel/blob/main/dotnet/samples/KernelSyntaxExamples/Example15_TextMemoryPlugin.cs
//or you can search using the ISemanticTextMemory interface: https://github.com/microsoft/semantic-kernel/blob/main/dotnet/samples/KernelSyntaxExamples/Example15_TextMemoryPlugin.cs
//In this example we will use Prompt Function to search for memories

const string recallFunctionDefinition = @"
Answer the question using the information stored in the memory. Use {{TextMemoryPlugin.Recall $input}} to recall information from the memory.
If you don't have sufficient information you reply with: I have nothing in my memory that I can use to answer your question.
When answering multiple questions, use a bullet point list.

Question: {{$input}}

Answer:
";

var oracle = kernel.CreateFunctionFromPrompt(recallFunctionDefinition, new OpenAIPromptExecutionSettings() { MaxTokens = 100 });


while (true)
{
    var query = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(query))
    {
        continue;
    }

    if (query.ToLower() == "end")
    {
        break;
    }

    result = await kernel.InvokeAsync(oracle, new()
    {
        [TextMemoryPlugin.InputParam] = query,
        [TextMemoryPlugin.CollectionParam] = "ExampleCollection",
        [TextMemoryPlugin.LimitParam] = "2",
        [TextMemoryPlugin.RelevanceParam] = "0.79",
    });

    Console.WriteLine($"Result: {result.GetValue<string>()}");

}

Console.WriteLine("Do you want to clear the memory?");
var shouldClear = Console.ReadLine();

if (!(shouldClear?.ToLower() ?? "n").StartsWith("y"))
{
    return;
}

Console.WriteLine("\nCleanup:");

var collections = memoryStore.GetCollectionsAsync();
await foreach (var collection in collections)
{
    Console.WriteLine(collection);
}
Console.WriteLine();

Console.WriteLine($"Removing Collection ExampleCollection");
await memoryStore.DeleteCollectionAsync("ExampleCollection");
Console.WriteLine();

#pragma warning restore SKEXP0021, SKEXP0003, SKEXP0011, SKEXP0052
