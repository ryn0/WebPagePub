using WebPagePub.Data.Enums;

namespace WebPagePub.Web.Models
{
    public class ContentSnippetEditModel
    {
        public int ContentSnippetId { get; set; }

        public SiteConfigSetting SnippetType { get; set; }

        public string Content { get; set; }
    }
}
