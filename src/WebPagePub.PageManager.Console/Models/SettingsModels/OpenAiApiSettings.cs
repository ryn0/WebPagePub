namespace WebPagePub.PageManager.Console.Models.SettingsModels
{
    public class OpenAiApiSettings
    {
        public required string ApiKey { get; set; }

        public int MaxTokens { get; set; } = 1000;

        public required string TextModel { get; set; }
    }
}