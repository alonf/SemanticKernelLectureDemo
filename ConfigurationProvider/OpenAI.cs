namespace ConfigurationProvider
{
    public static class OpenAI
    {
        public static string ApiKey => Environment.GetEnvironmentVariable("OPEN_AU_API_KEY") ?? throw new ArgumentException("OPEN_AU_API_KEY is not set");
        public static string ChatModelId => Environment.GetEnvironmentVariable("OPEN_AU_CHAT_MODEL_ID") ?? throw new ArgumentException("OPEN_AU_CHAT_MODEL_ID is not set");
        public static string EmbeddingModelId => "text-embedding-ada-002";
    }
}
