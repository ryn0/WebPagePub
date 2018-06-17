using System.ComponentModel.DataAnnotations;
using WebPagePub.Data.DbModels.BaseDbModels;

namespace WebPagePub.Data.Models.BaseModels
{
    public class UserStateInfo : StateInfo
    {
        [StringLength(36)]
        public string CreatedByUserId { get; set; }

        [StringLength(36)]
        public string UpdatedByUserId { get; set; }
    }
}
