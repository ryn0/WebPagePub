using System.Collections.Generic;

namespace WebPagePub.Data.Models.Transfer
{
    public class SiteMapDisplaySection
    {
        public string RelativePath { get; set; }

        public string PageTitle { get; set; }

        public IList<SiteMapDisplayItem> Items { get; set; }
    }
}