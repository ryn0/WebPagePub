using System.ComponentModel.DataAnnotations;

namespace WebPagePub.Web.Models
{
    public class EditSiteSectionModel
    {
        [Required]
        public string Title { get; set; }

        public int SiteSectionId { get; set; }

        public string BreadcrumbName { get;  set; }

        public bool IsHomePageSection { get; set; }
    }
}
