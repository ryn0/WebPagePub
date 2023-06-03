using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebPagePub.Web.Models
{
    public class EmailSubscribeEditModel
    {
        [EmailAddress]
        [Required]
        public string Email { get; set; }

        public int EmailSubscriptionId { get; set; }

        public bool IsSubscribed { get; set; }
    }
}
