using WebPagePub.Data.DbModels.BaseDbModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebPagePub.Data.Models.Db
{
    public class EmailSubscription : StateInfo
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EmailSubscriptionId { get; set; }

        [StringLength(100)]
        public string Email { get; set; }

        public bool IsSubscribed { get; set; }
    }
}
