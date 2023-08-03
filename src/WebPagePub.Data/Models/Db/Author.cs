using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebPagePub.Data.Models.BaseModels;

namespace WebPagePub.Data.Models.Db
{
    public class Author : UserStateInfo
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AuthorId { get; set; }

        [StringLength(15)]
        public string FirstName { get; set; }

        [StringLength(15)]
        public string LastName { get; set; }

        [StringLength(255)]
        public string AuthorBio { get; set; }

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
