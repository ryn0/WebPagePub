using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebPagePub.Web.Models
{

    public class LinkListModel
    {
        public List<LinkEditModel> Items { get; set; } = new List<LinkEditModel>();
    }
}
