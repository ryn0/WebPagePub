using WebPagePub.Data.DbModels.BaseDbModels;
using System.ComponentModel.DataAnnotations;

namespace WebPagePub.Data.Models
{
    public class ApplicationUser : ApplicationUserStateInfo
    {
        public ApplicationUser()
        {

        }

        [StringLength(36)]
        public override string Id
        {
            get
            {
                return base.Id;
            }

            set
            {
                base.Id = value;
            }
        }

        [StringLength(50)]
        public string AuthorName { get; set; }

        [StringLength(150)]
        public string AuthorUrl { get; set; }
    }
}
