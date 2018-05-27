using WebPagePub.Data.DbModels.BaseDbModels;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebPagePub.Data.Models
{
    public class Tag : StateInfo
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TagId { get; set; }

        [StringLength(75)]
        public string Name { get; set; }

        [StringLength(75)]
        public string Key { get; set; }

        public virtual ICollection<SitePageTag> SitePageTags { get; set; }
    }
}
