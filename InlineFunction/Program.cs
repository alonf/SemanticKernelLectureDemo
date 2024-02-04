using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Diagnostics;
using System.IO;

Console.WriteLine("======== Inline Function Definition ========");

var kernel = Kernel.CreateBuilder()
    .AddOpenAIChatCompletion(
        modelId: ConfigurationProvider.OpenAI.ChatModelId,
        apiKey: ConfigurationProvider.OpenAI.ApiKey)
    .Build();

// Function defined using few-shot design pattern
string promptTemplate = @"
אתה מומחה לספק סיבות ותירוצים לאיחורים ופיספוסים של אירועים. 
תהיה יצירתי ומצחיק. ככל שהתרחיש יותר דמיוני הוא יותר מצחיק.

דוגמאות:
אירוע: איחרתי לשיעור בכיתה
תירוץ: חייזרים באו לבקר אותי

אירוע: פספסתי מסיבת יום הולדת של חבר טוב
תירוץ: חיכיתי שתסתיים ריצה של תוכנית עם רקורסיה ולא היה תנאי עצירה

אירוע:
{{$input}}
";

var excuseFunction = kernel.CreateFunctionFromPrompt(promptTemplate, new OpenAIPromptExecutionSettings() { MaxTokens = 500, Temperature = 1.5, TopP = 1 });

var input1 = "אחרתי לפגישה עיוורת עם בחורה";
var result1 = await kernel.InvokeAsync(excuseFunction, new() { ["input"] = input1 });
WriteToFile("result.txt", $"{input1}: {result1.GetValue<string>() ?? ""}");

var input2 = "מצטער, אשתי, שכחתי להוציא את הילד מהגן";
var result2 = await kernel.InvokeAsync(excuseFunction, new() { ["input"] = input2 } );
WriteToFile("result.txt", $"{input2}: {result2.GetValue<string>() ?? ""}");

var input3 = $"Translate this date {DateTimeOffset.Now:f} to Hebrew format and write it in Hebrew";
var fixedFunction = kernel.CreateFunctionFromPrompt(input3, new OpenAIPromptExecutionSettings() { MaxTokens = 500 });
var result3 = await kernel.InvokeAsync(fixedFunction);
WriteToFile("result.txt", result3.GetValue<string>() ?? "");


Process.Start("notepad.exe", "result.txt");

void WriteToFile(string fileName, string content)
{
    using (StreamWriter writer = new StreamWriter(fileName, true))
    {
        writer.WriteLine(content);
    }
}