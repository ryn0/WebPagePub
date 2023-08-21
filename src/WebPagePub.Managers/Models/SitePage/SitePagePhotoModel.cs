namespace WebPagePub.Managers.Models.SitePages
{
    public class SitePagePhotoModel
    {
        public int SitePagePhotoId { get; set; }

        public string PhotoOriginalUrl { get; set; } = default!;

        public string PhotoThumbUrl { get; set; } = default!;

        public string PhotoOriginalCdnUrl { get; set; } = default!;

        public string PhotoThumbCdnUrl { get; set; } = default!;

        public string Title { get; set; } = default!;

        public string Description { get; set; } = default!;

        public bool IsDefault { get; set; }

        public string PhotoPreviewUrl { get; set; } = default!;

        public string PhotoPreviewCdnUrl { get; set; } = default!;

        public string PhotoFullScreenUrl { get; set; } = default!;

        public string PhotoFullScreenCdnUrl { get; set; } = default!;

        public string FileName { get; set; } = default!;
    }
}
