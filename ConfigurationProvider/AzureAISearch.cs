namespace ConfigurationProvider;

public static class AzureAISearch
{
    public static string Endpoint => Environment.GetEnvironmentVariable("AZURE_AI_SEARCH_ENDPOINT") ?? throw new ArgumentException("AZURE_AI_SEARCH_ENDPOINT is not set");
    public static string ApiKey => Environment.GetEnvironmentVariable("AZURE_AI_SEARCH_API_KEY") ?? throw new ArgumentException("AZURE_AI_SEARCH_API_KEY is not set");
}