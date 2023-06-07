namespace WebPagePub.Web.Models
{
    public class PreviousAndNextModel
    {
        public string PreviousName { get; set; } = default!;
        public string PreviousUrlPath { get; set; } = default!;
        public string DefaultPreviousPhotoThumbCdnUrl { get; set; } = default!;
        public string NextName { get; set; } = default!;
        public string NextUrlPath { get; set; } = default!;
        public string DefaultNextPhotoThumbCdnUrl { get; set; } = default!;
    }
}
