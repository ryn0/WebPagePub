using System.ComponentModel.DataAnnotations;

namespace WebPagePub.Web.Models
{
    public class SiteSectionEditModel
    {
        [Display(Name = "Title")]
        [Required]
        public string Title { get; set; } = default!;

        [Display(Name = "Site Section ID")]
        public int SiteSectionId { get; set; }

        [Display(Name = "Breadcrumb Name")]
        public string BreadcrumbName { get;  set; } = default!;

        [Display(Name = "Is Home Page Section")]
        public bool IsHomePageSection { get; set; }

        [Display(Name = "Key")]
        public string Key { get; set; } = default!;
    }
}
