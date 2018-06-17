using WebPagePub.Data.Enums;

namespace WebPagePub.Services.Models
{
    public class ContentSnippetModel
    {
        public int ContentSnippetId { get; set; }

        public SiteConfigSetting SnippetType { get; set; }

        public string Content { get; set; }
    }
}
