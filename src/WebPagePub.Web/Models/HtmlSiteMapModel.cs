using System.Collections.Generic;

namespace WebPagePub.Web.Models
{
    public class HtmlSiteMapModel
    {
        public List<SectionPage> SectionPages { get; set; } = new List<SectionPage>();
    }

    public class SectionPage
    {
        public string CanonicalUrl { get; set; }

        public string AnchorText { get; set; }

        public bool HasChildren
        {
            get
            {
                return ChildPages.Count > 0;
            }
        }

        public List<SectionPage> ChildPages { get; set; } = new List<SectionPage>();
    }
}
