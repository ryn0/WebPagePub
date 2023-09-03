namespace WebPagePub.ChatCommander.SettingsModels
{
    public class ArticleFromUrlGeneratorModel
    {
        public int MinutesOffsetForArticleMin { get; set; }
        public int MinutesOffsetForArticleMax { get; set; }
        public string SectionKey { get; set; } = string.Empty;
        public string SiteMapUrl { get; set; } = string.Empty;
        public string SiteMapUrlContentIncludeWords { get; set; } = string.Empty;
        public string ContentExtractionXPath { get; set; } = string.Empty;
    }
}