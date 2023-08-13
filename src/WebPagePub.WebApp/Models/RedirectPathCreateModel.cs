using System.ComponentModel.DataAnnotations;

namespace WebPagePub.Web.Models
{
    public class RedirectPathCreateModel
    {
        [Display(Name = "Path")]
        [Required]
        public string Path { get; set; } = default!;

        [Display(Name = "Path Destination")]
        [Required]
        public string PathDestination { get; set; } = default!;
    }
}
