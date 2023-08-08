namespace WebPagePub.Web.Models
{
    public class SitePageContentModel
    {
        private string canonicalUrl = default!;

        public string CanonicalUrl
        {
            get
            {
                return this.canonicalUrl;
            }
            set
            {
                this.canonicalUrl = value;

                if (!string.IsNullOrWhiteSpace(this.canonicalUrl))
                {
                    this.canonicalUrl = this.canonicalUrl.TrimEnd('/');
                }
            }
        }

        public string PhotoOriginalUrl { get; set; } = default!;

        public int PhotoUrlHeight { get; set; }

        public int PhotoUrlWidth { get; set; }

        public string Title { get; set; } = default!;

        public string BreadcrumbName { get; set; } = default!;

        public string Key { get; set; } = default!;

        public string UrlPath { get; set; } = default!;

        public string Content { get; set; } = default!;

        public bool IsIndex
        {
            get
            {
                return this.Key.ToLower() == "index";
            }
        }

        public string MetaKeywords { get; set; } = default!;

        public string MetaDescription { get; set; } = default!;

        public DateTime PublishedDateTimeUtc { get; set; }

        public DateTime LastUpdatedDateTimeUtc { get; set; }

        public string FriendlyPublishDateDisplay
        {
            get
            {
                return this.FormatDate(this.PublishedDateTimeUtc);
            }
        }

        public string FriendlyLastUpdateDateDisplay
        {
            get
            {
                if (this.LastUpdatedDateTimeUtc > this.PublishedDateTimeUtc)
                {
                    return this.FormatDate(this.LastUpdatedDateTimeUtc);
                }
                else
                {
                    return this.FormatDate(this.PublishedDateTimeUtc);
                }
            }
        }
        public string DefaultPhotoOriginalUrl { get; set; } = default!;

        public string DefaultPhotoOriginalCdnUrl { get; set; } = default!;

        public List<string> Tags { get; set; } = new List<string>();

        public List<SitePagePhotoModel> Photos { get; set; } = new List<SitePagePhotoModel>();

        public string DefaultPhotoThumbUrl { get; set; } = default!;

        public string DefaultPhotoThumbCdnUrl { get; set; } = default!;
        public string PreviousUrlPath { get; set; } = default!;
        public string NextUrlPath { get; set; } = default!;
        public string PreviousName { get; set; } = default!;
        public string NextName { get; set; } = default!;
        public string DefaultPreviousPhotoThumbCdnUrl { get; set; } = default!;
        public string DefaultNextPhotoThumbCdnUrl { get; set; } = default!;

        public string PageHeader { get; set; } = default!;
        public string SectionKey { get;  set; } = default!;

        private string FormatDate(DateTime date)
        {
            var dt = date;

            string suffix;

            switch (dt.Day)
            {
                case 1:
                case 21:
                case 31:
                    suffix = "st";
                    break;
                case 2:
                case 22:
                    suffix = "nd";
                    break;
                case 3:
                case 23:
                    suffix = "rd";
                    break;
                default:
                    suffix = "th";
                    break;
            }

            return string.Format("{0:MMMM} {1}{2}, {0:yyyy}", dt, dt.Day, suffix);
        }
    }
}
