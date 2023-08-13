using Microsoft.AspNetCore.Mvc.Rendering;
using WebPagePub.Data.Enums;

namespace WebPagePub.WebApp.Models.ContentSnippet
{
    public class ContentSnippetEditModel
    {
        public int ContentSnippetId { get; set; }

        public SiteConfigSetting SnippetType { get; set; }

        public string? Content { get; set; }

        public List<SelectListItem> UnusedSnippetTypes { get; set; } = new List<SelectListItem>();
    }
}
