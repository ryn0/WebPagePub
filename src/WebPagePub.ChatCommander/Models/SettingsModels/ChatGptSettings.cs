namespace WebPagePub.ChatCommander.Models.SettingsModels
{
    public class ChatGptSettings
    {
        public required string ApiKey { get; set; }

        public int MaxTokens { get; set; }

        public required string TextModel { get; set; }
    }
}