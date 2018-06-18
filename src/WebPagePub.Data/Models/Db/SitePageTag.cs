using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebPagePub.Data.DbModels.BaseDbModels;

namespace WebPagePub.Data.Models
{
    public class SitePageTag : StateInfo
    {
        [Column(Order = 0)]
        [Key]
        [ForeignKey(nameof(SitePage))]
        public int SitePageId { get; set; }

        public virtual SitePage SitePage { get; set; }

        [Column(Order = 1)]
        [Key]
        [ForeignKey(nameof(Tag))]
        public int TagId { get; set; }

        public Tag Tag { get; set; }
    }
}
