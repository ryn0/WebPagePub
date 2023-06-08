namespace WebPagePub.Web.Helpers
{
    public class SiteMapItem
    {
        public string Url { get; set; } = default!;

        public DateTime LastMode { get; set; }

        public ChangeFrequency ChangeFrequency { get; set; }

        public double Priority { get; set; }

        public List<string> ImageUrls { get; set; }

        public bool HasImage()
        {
            return ImageUrls != null && ImageUrls.Any();
        }
    }
}
