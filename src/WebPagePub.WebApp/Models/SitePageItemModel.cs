using System;
using System.Collections.Generic;

namespace WebPagePub.Web.Models
{
    public class SitePageItemModel
    {
        public int SitePageId { get; set; }

        public bool IsIndex { get; set; }

        public DateTime CreateDate { get; set; }

        public string Title { get; set; }

        public string Key { get; set; }

        public bool IsLive { get;  set; }

        public string LiveUrlPath { get; set; }

        public string PreviewUrlPath { get; set; }

        public bool IsSiteSection { get; set; }

        public int SitePageSectionId { get;  set; }
    }
}
