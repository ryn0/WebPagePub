using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebPagePub.Data.DbModels.BaseDbModels;

namespace WebPagePub.Data.Models.Db
{
    public class LinkRedirection : StateInfo
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LinkRedirectionId { get; set; }

        [StringLength(75)]
        public string LinkKey { get; set; }

        public string UrlDestination { get; set; }
    }
}
