using System.ComponentModel.DataAnnotations;
using WebPagePub.Data.Enums;

namespace WebPagePub.WebApp.Models.SitePage
{
    public class SitePageCommentDisplayModel
    {
        public DateTime CreateDate { get; set; }

        [Display(Name = "Email")]
        [EmailAddress]
        [Required]
        public string Email { get; set; } = default!;

        [Display(Name = "Website (optional)")]
        [Url]
        public string? Website { get; set; } = default;

        [Display(Name = "Name")]
        [Required]
        public string Name { get; set; } = default!;

        [Display(Name = "Comment")]
        [Required]
        public string Comment { get; set; } = default!;
    }
}
