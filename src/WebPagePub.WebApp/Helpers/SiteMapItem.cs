using System;

namespace WebPagePub.Web.Helpers
{
    public class SiteMapItem
    {
        public string Url { get; set; }

        public DateTime LastMode { get; set; }

        public ChangeFrequency ChangeFrequency { get; set; }

        public double Priority { get; set; }
    }
}
