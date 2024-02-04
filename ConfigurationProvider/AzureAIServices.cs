namespace ConfigurationProvider;

public static class AzureAIServices
{
    public static string ApiKey => Environment.GetEnvironmentVariable("AZURE_AI_API_KEY") ?? throw new ArgumentException("AZURE_AI_API_KEY is not set");
    public static string Endpoint => Environment.GetEnvironmentVariable("AZURE_AI_ENDPOINT") ?? throw new ArgumentException("AZURE_AI_ENDPOINT is not set");
    public static string DeploymentName => Environment.GetEnvironmentVariable("AZURE_DEPLOYMENT_NAME") ?? throw new ArgumentException("AZURE_DEPLOYMENT_NAME is not set");
}