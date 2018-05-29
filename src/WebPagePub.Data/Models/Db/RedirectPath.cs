using System.ComponentModel.DataAnnotations.Schema;
using WebPagePub.Data.DbModels.BaseDbModels;

namespace WebPagePub.Data.Models.Db
{
    public class RedirectPath : CreatedStateInfo
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RedirectPathId { get; set; }

        public string Path { get; set; }

        public string PathDestination { get; set; }
    }
}
