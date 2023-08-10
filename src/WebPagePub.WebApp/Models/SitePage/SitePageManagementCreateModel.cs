using System.ComponentModel.DataAnnotations;

namespace WebPagePub.WebApp.Models.SitePage
{
    public class SitePageManagementCreateModel
    {
        public string Title { get; set; } = default!;

        [Required]
        public int SiteSectionId { get; set; }

        public string? SiteSectionKey { get; set; }
    }
}
