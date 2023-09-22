using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebPagePub.Data.DbModels.BaseDbModels;

namespace WebPagePub.Data.Models.Db
{
    public class ClickLog : CreatedStateInfo
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ClickLogId { get; set; }

        [StringLength(75)]
        public string IpAddress { get; set; }

        public string UserAgent { get; set; }

        public string Headers { get; set; }

        public string Url { get; set; }
    }
}
