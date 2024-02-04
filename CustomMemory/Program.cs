using System;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Memory;

Console.WriteLine("====================================================");
Console.WriteLine("======== Semantic Memory (volatile, in RAM) ========");
Console.WriteLine("====================================================");


#pragma warning disable SKEXP0011, SKEXP0003, SKEXP0052
var memoryWithCustomDb = new MemoryBuilder()

    .WithOpenAITextEmbeddingGeneration("text-embedding-ada-002", ConfigurationProvider.OpenAI.ApiKey)
    .WithMemoryStore(new VolatileMemoryStore())
    .Build();

Console.WriteLine("\nEnter 5 sections of text:");

for (int i = 0; i < 5; i++)
{
    Console.Write($"#{i + 1}: ");
    var text = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(text))
    {
        --i;
        continue;
    }

    await memoryWithCustomDb.SaveReferenceAsync(
               collection: "ExampleCollection",
                      externalSourceName: "Console",
                      externalId: i.ToString(),
                      description: text,
                      text: text);
}

Console.WriteLine("Enter text to search. Enter End to stop");
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


    var memoryResults = memoryWithCustomDb.SearchAsync("ExampleCollection", query, limit: 2, minRelevanceScore: 0.5);

    int i = 0;
    await foreach (MemoryQueryResult memoryResult in memoryResults)
    {
        Console.WriteLine($"Result {++i}:");
        Console.WriteLine("  Id:     : " + memoryResult.Metadata.Id);
        Console.WriteLine("  Title    : " + memoryResult.Metadata.Description);
        Console.WriteLine("  Relevance: " + memoryResult.Relevance);
        Console.WriteLine();
    }

    Console.WriteLine("----------------------");
}


Console.WriteLine("\nBye Bye");

#pragma warning restore SKEXP0011, SKEXP0003, SKEXP0052
