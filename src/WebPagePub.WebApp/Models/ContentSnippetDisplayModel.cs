using WebPagePub.Data.Enums;

namespace WebPagePub.Web.Models
{
    public class ContentSnippetDisplayModel
    {
        public SiteConfigSetting SnippetType { get; set; }

        public string Content { get; set; }
    }
}