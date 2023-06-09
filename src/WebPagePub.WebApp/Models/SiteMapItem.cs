using WebPagePub.Web.Helpers;

namespace WebPagePub.WebApp.Models
{
    public class SiteMapItem
    {
        public string Url { get; set; } = default!;

        public DateTime LastMode { get; set; }

        public ChangeFrequency ChangeFrequency { get; set; }

        public double Priority { get; set; }

        public List<SiteMapImageItem> Images { get; set; }

        public bool HasImage()
        {
            return Images != null && Images.Any();
        }
    }
}
