using System.ComponentModel.DataAnnotations;

namespace WebPagePub.WebApp.Models.Author
{
    public class AuthorEditModel
    {
        public int AuthorId { get; set; }

        [StringLength(15)]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(15)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(255)]
        public string AuthorBio { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string PhotoOriginalUrl { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string PhotoThumbUrl { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string PhotoPreviewUrl { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string PhotoFullScreenUrl { get; set; } = string.Empty;
    }
}
