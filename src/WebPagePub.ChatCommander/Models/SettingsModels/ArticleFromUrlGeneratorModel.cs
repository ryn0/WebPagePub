namespace WebPagePub.ChatCommander.SettingsModels
{
    public class ArticleFromUrlGeneratorModel
    {
        public int MinutesOffsetForArticleMin { get; set; }
        public int MinutesOffsetForArticleMax { get; set; }
        public string SectionKey { get; set; }
        public string SiteMapUrl { get; set; }
        public string SiteMapUrlContentIncludeWords { get; set; }
        public string ContentExtractionXPath { get; set; }
    }
}