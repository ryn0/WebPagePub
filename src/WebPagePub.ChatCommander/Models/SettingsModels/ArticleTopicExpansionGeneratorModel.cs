namespace WebPagePub.ChatCommander.Models.SettingsModels
{
    public class ArticleTopicExpansionGeneratorModel
    {
        public int MinutesOffsetForArticleMin { get; set; }
        public int MinutesOffsetForArticleMax { get; set; }
        public int QuestionQuantity { get; set; }
        public string Topic { get; set; } = string.Empty;
        public string SectionKey { get; set; } = string.Empty;
    }
}
