namespace WebPagePub.Web.Models
{
    public class SitePagePhotoModel
    {
        public int SitePagePhotoId { get; set; }

        public string PhotoUrl { get; set; }

        public string PhotoThumbUrl { get; set; }

        public string PhotoCdnUrl { get; set; }

        public string PhotoThumbCdnUrl { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public bool IsDefault { get; set; }

        public string PhotoPreviewUrl { get; internal set; }

        public string PhotoPreviewCdnUrl { get; internal set; }

        public string PhotoFullScreenUrl { get; set; }

        public string PhotoFullScreenCdnUrl { get; set; }

        public string FileName { get; set; }
    }
}
