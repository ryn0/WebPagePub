namespace WebPagePub.PageManager.Console.Models.SettingsModels
{
    public class ArticleFromKeywordsGeneratorModel
    {
        public int MinutesOffsetForArticleMin { get; set; }
        public int MinutesOffsetForArticleMax { get; set; }
        public string SectionKey { get; set; } = string.Empty;
    }
}
