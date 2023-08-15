using WebPagePub.Core.Utilities;

namespace WebPagePub.WebApp.Models.SitePage
{
    public class SitePageContentModel
    {
        private string canonicalUrl = default!;

        public string CanonicalUrl
        {
            get
            {
                return canonicalUrl;
            }
            set
            {
                canonicalUrl = value;

                if (!string.IsNullOrWhiteSpace(canonicalUrl))
                {
                    canonicalUrl = canonicalUrl.TrimEnd('/');
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
                return Key.ToLower() == "index";
            }
        }

        public string MetaKeywords { get; set; } = default!;

        public string MetaDescription { get; set; } = default!;

        public DateTime PublishedDateTimeUtc { get; set; }

        public DateTime LastUpdatedDateTimeUtc { get; set; }

        public string LastUpdatedDateTimeUtcIso
        {
            get
            {
                if (LastUpdatedDateTimeUtc > PublishedDateTimeUtc)
                {
                    return DateUtilities.UtcFormatDate(LastUpdatedDateTimeUtc);
                }
                else
                {
                    return DateUtilities.UtcFormatDate(PublishedDateTimeUtc);
                }
            }
        }

        public string FriendlyPublishDateDisplay
        {
            get
            {
                return DateUtilities.FriendlyFormatDate(PublishedDateTimeUtc);
            }
        }

        public string FriendlyLastUpdateDateDisplay
        {
            get
            {
                if (LastUpdatedDateTimeUtc > PublishedDateTimeUtc)
                {
                    return DateUtilities.FriendlyFormatDate(LastUpdatedDateTimeUtc);
                }
                else
                {
                    return DateUtilities.FriendlyFormatDate(PublishedDateTimeUtc);
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
        public string SectionKey { get; set; } = default!;
        public bool ExcludePage { get; set; }
    }
}
