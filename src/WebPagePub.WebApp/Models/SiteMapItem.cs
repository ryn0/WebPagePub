using WebPagePub.WebApp.Enums;

namespace WebPagePub.WebApp.Models
{
    public class SiteMapItem
    {
        public string Url { get; set; } = default!;

        public DateTime LastMode { get; set; }

        public ChangeFrequency ChangeFrequency { get; set; }

        public double Priority { get; set; }

        public List<SiteMapImageItem> Images { get; set; } = new List<SiteMapImageItem>();

        public bool HasImage()
        {
            return this.Images != null && this.Images.Any();
        }
    }
}
