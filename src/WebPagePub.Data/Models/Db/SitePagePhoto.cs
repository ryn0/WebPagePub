using WebPagePub.Data.DbModels.BaseDbModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebPagePub.Data.Models
{
    public class SitePagePhoto : StateInfo
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SitePagePhotoId { get; set; }

        [Required]
        [StringLength(255)]
        public string PhotoUrl { get; set; }

        [Required]
        [StringLength(255)]
        public string PhotoThumbUrl { get; set; }

        [Required]
        [StringLength(255)]
        public string PhotoPreviewUrl { get; set; }

        [Required]
        [StringLength(255)]
        public string PhotoFullScreenUrl { get; set; }

        public int PhotoFullScreenUrlHeight { get; set; }

        public int PhotoFullScreenUrlWidth { get; set; }

        public bool IsDefault { get; set; }

        public int Rank { get; set; }

        [StringLength(100)]
        public string Title { get; set; }

        public string Description { get; set; }

        [ForeignKey(nameof(SitePage))]
        public int SitePageId { get; set; }

        public virtual SitePage SitePage  { get; set; }
    }
}
