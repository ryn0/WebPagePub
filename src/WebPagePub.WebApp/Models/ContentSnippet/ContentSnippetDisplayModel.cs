using WebPagePub.Data.Enums;

namespace WebPagePub.WebApp.Models.ContentSnippet
{
    public class ContentSnippetDisplayModel
    {
        public SiteConfigSetting SnippetType { get; set; }

        public string Content { get; set; } = string.Empty;
    }
}