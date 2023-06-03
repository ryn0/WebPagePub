using System.ComponentModel.DataAnnotations;

namespace WebPagePub.Web.Models
{
    public class LinkEditModel
    {
        public int LinkRedirectionId { get; set; }

        [StringLength(75)]
        public string LinkKey { get; set; }

        public string UrlDestination { get; set; }
    }
}
