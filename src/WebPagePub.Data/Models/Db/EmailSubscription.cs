using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebPagePub.Data.DbModels.BaseDbModels;

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
