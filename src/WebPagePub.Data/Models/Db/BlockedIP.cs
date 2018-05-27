using WebPagePub.Data.DbModels.BaseDbModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebPagePub.Data.Models.Db
{
    public class BlockedIP : CreatedStateInfo
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BlockedIPId { get; set; }

        [StringLength(75)]
        public string IpAddress { get; set; }
    }
}
