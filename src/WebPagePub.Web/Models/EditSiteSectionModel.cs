using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebPagePub.Web.Models
{
    public class EditSiteSectionModel
    {
        public string Title { get; set; }

        public int SiteSectionId { get; set; }
        public string BreadcrumbName { get;  set; }
    }
}
