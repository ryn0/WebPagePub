namespace WebPagePub.Data.Enums
{
    public enum SiteConfigSetting
    {
        Unknown = 0,
        GoogleSiteVerification = 1,
        CdnPrefixWithProtocol = 2,
        LogoUrl = 3,
        FacebookUrl = 4,
        YouTubeUrl = 5,
        TwitterUrl = 6,
        WebsiteName = 7,
        GoogleAnalytics = 8,
        IisWebsiteName = 9,
        CanonicalDomain = 10,
        BlobPrefix = 11,
        AzureStorageConnectionString = 12,
        HeaderHtml = 13,
        FooterHtml = 14,
        MenuHtml = 15,
        InstagramUrl = 16,
        AboveReview = 17
    }

    public enum PageType
    {
        Unknown = 0,
        Content = 1,
        Review = 2,
        PageList = 3,
        Photo = 4,
        Informational = 5
    }

    public enum CommentStatus : byte
    {
        Unknown = 0,
        AwaitingModeration = 1,
        Rejected = 2,
        Approved = 3,
        Removed = 4,
        Spam = 5
    }
}
