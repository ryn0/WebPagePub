using WebPagePub.Data.Enums;
using System.Collections.Generic;

namespace WebPagePub.Services.Models
{
    public class ContentSnippetModel
    {
        public int ContentSnippetId { get; set; }

        public SiteConfigSetting SnippetType { get; set; }

        public string Content { get; set; }

    }

    public class ContentSnippetListModel
    {
        public List<ContentSnippetModel> Items { get; set; } = new List<ContentSnippetModel>();
    }
}
