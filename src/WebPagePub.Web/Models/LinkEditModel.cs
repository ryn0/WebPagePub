using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebPagePub.Web.Models
{
    public class LinkEditModel
    { 
        public int LinkRedirectionId { get; set; }

        [StringLength(75)]
        public string LinkKey { get; set; }

        public string UrlDestination { get; set; }
    }

    public class LinkListModel
    {
        public List<LinkEditModel> Items { get; set; } = new List<LinkEditModel>();
    }
}
