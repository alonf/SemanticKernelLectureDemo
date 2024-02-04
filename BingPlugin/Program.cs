using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.Core;
using Microsoft.SemanticKernel.Plugins.Web;
using Microsoft.SemanticKernel.Plugins.Web.Bing;

Console.WriteLine("======== Inline Function Definition ========");

/*
 * Example: normally you would place prompt templates in a folder to separate
 *          C# code from natural language code, but you can also define a semantic
 *          function inline if you like.
 */
var kernel = Kernel.CreateBuilder()
    .AddOpenAIChatCompletion(
        modelId: ConfigurationProvider.OpenAI.ChatModelId,
        apiKey: ConfigurationProvider.OpenAI.ApiKey)
    .Build();

string bingApiKey = ConfigurationProvider.Bing.ApiKey;

#pragma warning disable SKEXP0054
var bingConnector = new BingConnector(bingApiKey);
var bing = new WebSearchEnginePlugin(bingConnector);
#pragma warning restore SKEXP0054
kernel.ImportPluginFromObject(bing, "bing");

Console.WriteLine("======== Bing Search Plugins ========");

// Run
var question = "Tell me about LLM";
var function = kernel.Plugins["bing"]["search"];
var result = await kernel.InvokeAsync(function, new() { ["query"] = question });

Console.WriteLine(question);
Console.WriteLine(result.GetValue<string>());

Console.ReadKey();

Console.WriteLine("======== Use Search Plugin to answer user questions ========");

const string semanticFunction = @"Answer questions only when you know the facts or the information is provided.
When you don't have sufficient information you reply with a list of commands to find the information needed.
When answering multiple questions, use a bullet point list.
Note: make sure single and double quotes are escaped using a backslash char.

[COMMANDS AVAILABLE]
- bing.search

[INFORMATION PROVIDED]
{{ $externalInformation }}

[EXAMPLE 1]
Question: what's the biggest lake in Italy?
Answer: Lake Garda, also known as Lago di Garda.

[EXAMPLE 2]
Question: what's the biggest lake in Italy? What's the smallest positive number?
Answer:
* Lake Garda, also known as Lago di Garda.
* The smallest positive number is 1.

[EXAMPLE 3]
Question: what's Ferrari stock price? Who is the current number one female tennis player in the world?
Answer:
{{ '{{' }} bing.search ""what\\'s Ferrari stock price?"" {{ '}}' }}.
{{ '{{' }} bing.search ""Who is the current number one female tennis player in the world?"" {{ '}}' }}.

[END OF EXAMPLES]

[TASK]
Question: {{ $question }}.
Answer: ";

question = "How many users are registered to Azure Israel Meetup?";
Console.WriteLine(question);

var oracle = kernel.CreateFunctionFromPrompt(semanticFunction, new OpenAIPromptExecutionSettings() { MaxTokens = 150, Temperature = 0, TopP = 1 });

var kernelAnswer = await kernel.InvokeAsync(oracle, new KernelArguments()
{
    ["question"] = question,
    ["externalInformation"] = string.Empty
});

var answer = kernelAnswer.GetValue<string>()!;

// If the answer contains commands, execute them using the prompt renderer.
if (answer.Contains("bing.search", StringComparison.OrdinalIgnoreCase))
{
    var promptTemplateFactory = new KernelPromptTemplateFactory();
    var promptTemplate = promptTemplateFactory.Create(new PromptTemplateConfig(answer));

    Console.WriteLine("---- Fetching information from Bing...");
    var information = await promptTemplate.RenderAsync(kernel);

    Console.WriteLine("Information found:");
    Console.WriteLine(information);

    // Run the prompt function again, now including information from Bing
    kernelAnswer = await kernel.InvokeAsync(oracle, new KernelArguments()
    {
        ["question"] = question,
        // The rendered prompt contains the information retrieved from search engines
        ["externalInformation"] = information
    });
}
else
{
    Console.WriteLine("AI had all the information, no need to query Bing.");
}

Console.WriteLine("---- ANSWER:");
Console.WriteLine(kernelAnswer.GetValue<string>());