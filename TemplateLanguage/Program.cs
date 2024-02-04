using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.Core;

Console.WriteLine("======== Template and Builtin Plugin ========");

//https://learn.microsoft.com/en-us/semantic-kernel/agents/plugins/out-of-the-box-plugins?tabs=Csharp
//https://github.com/microsoft/semantic-kernel/blob/main/dotnet/src/Plugins/Plugins.Core/TimePlugin.cs


var kernel = Kernel.CreateBuilder()
    .AddOpenAIChatCompletion(
        modelId: ConfigurationProvider.OpenAI.ChatModelId,
        apiKey: ConfigurationProvider.OpenAI.ApiKey)
    .Build();

#pragma warning disable SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
kernel.ImportPluginFromType<TimePlugin>("time");
#pragma warning restore SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

// Prompt Function invoking time.Date and time.Time method functions
const string FunctionDefinition = @"
Today is: {{time.Date}}
Current time is: {{time.Time}}

Answer to the following questions using JSON syntax, including the data used.
Is it morning, afternoon, evening, or night (morning/afternoon/evening/night)?
Is it weekend time (weekend/not weekend)?
";

// This allows to see the prompt before it's sent to OpenAI
Console.WriteLine("--- Rendered Prompt");
var promptTemplateFactory = new KernelPromptTemplateFactory();
var promptTemplate = promptTemplateFactory.Create(new PromptTemplateConfig(FunctionDefinition));
var renderedPrompt = await promptTemplate.RenderAsync(kernel);
Console.WriteLine(renderedPrompt);

// Run the prompt / prompt function
var kindOfDay = kernel.CreateFunctionFromPrompt(FunctionDefinition, new OpenAIPromptExecutionSettings() { MaxTokens = 100 });

// Show the result
Console.WriteLine("--- Prompt Function result");
var result = await kernel.InvokeAsync(kindOfDay);
Console.WriteLine(result.GetValue<string>());