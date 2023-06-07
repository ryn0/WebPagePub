using System.ComponentModel.DataAnnotations;

namespace WebPagePub.Web.Models
{
    public class SitePageManagementCreateModel
    {
        public string Title { get; set; } = default!;

        [Required]
        public int SiteSectionId { get; set; }
    }
}
