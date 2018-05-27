using WebPagePub.Data.Enums;
using System.Collections.Generic;

namespace WebPagePub.Web.Models
{
    public class ContentSnippetEditModel
    {
        public int ContentSnippetId { get; set; }

        public SiteConfigSetting SnippetType { get; set; }

        public string Content { get; set; }
         
    }

    public class ContentSnippetEditListModel
    {
        public List<ContentSnippetEditModel> Items { get; set; } = new List<ContentSnippetEditModel>();
    }
}
