using System.ComponentModel.DataAnnotations;

namespace WebPagePub.Web.Models
{
    public class RedirectPathCreateModel
    {
        [Required]
        public string Path { get; set; } = default!;

        [Required]
        public string PathDestination { get; set; } = default!;
    }
}
