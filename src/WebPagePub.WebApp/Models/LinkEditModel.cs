using System.ComponentModel.DataAnnotations;

namespace WebPagePub.Web.Models
{
    public class LinkEditModel
    {
        [Display(Name = "Link Redirection ID")]
        public int LinkRedirectionId { get; set; }

        [Display(Name = "Key")]
        [Required]
        [StringLength(75)]
        public string LinkKey { get; set; } = string.Empty;

        [Display(Name = "URL Destination")]
        [Required]
        public string UrlDestination { get; set; } = string.Empty;
    }
}
