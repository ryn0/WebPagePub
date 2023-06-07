namespace WebPagePub.Web.Helpers
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

        public static string GetCurrentDomain(HttpContext httpContext)
        {
            return string.Format("{0}{1}{2}",
                                    httpContext.Request.Scheme,
                                    "://",
                                    httpContext.Request.Host.ToUriComponent());
        }
    }
}