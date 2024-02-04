namespace ConfigurationProvider;

public static class Bing
{
    public static string ApiKey => Environment.GetEnvironmentVariable("BING_API_KEY") ?? throw new ArgumentException("BING_API_KEY is not set");
}