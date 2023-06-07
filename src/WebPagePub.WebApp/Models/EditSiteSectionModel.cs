using System.ComponentModel.DataAnnotations;

namespace WebPagePub.Web.Models
{
    public class EditSiteSectionModel
    {
        [Required]
        public string Title { get; set; } = default!;

        public int SiteSectionId { get; set; }

        public string BreadcrumbName { get;  set; } = default!;

        public bool IsHomePageSection { get; set; }
    }
}
