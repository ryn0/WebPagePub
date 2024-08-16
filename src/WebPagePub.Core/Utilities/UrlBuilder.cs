namespace WebPagePub.Core
{
    public class UrlBuilder
    {
        public static string BlogUrlPath(string sectionKey, string pageKey)
        {
            return string.Format("/{0}/{1}", sectionKey, pageKey);
        }

        public static string BlogPreviewUrlPath(int sitePageId)
        {
            return string.Format(
                "/preview/{0}",
                sitePageId);
        }

        public static string ConvertBlobToCdnUrl(
           string blobUrl,
           string blobPrefix,
           string cdnPrefix)
        {
            if (string.IsNullOrWhiteSpace(blobUrl) ||
                string.IsNullOrWhiteSpace(blobPrefix) ||
                string.IsNullOrWhiteSpace(cdnPrefix))
            {
                return null;
            }

            // Normalize inputs to remove trailing slashes
            blobUrl = RemoveTrailingSlash(blobUrl);
            blobPrefix = RemoveTrailingSlash(blobPrefix);
            cdnPrefix = RemoveTrailingSlash(cdnPrefix);

            return blobUrl.Replace(blobPrefix, cdnPrefix);
        }

        private static string RemoveTrailingSlash(string input)
        {
            return input.EndsWith("/") ? input.TrimEnd('/') : input;
        }
    }
}