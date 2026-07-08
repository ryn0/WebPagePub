namespace WebPagePub.Web.Helpers
{
    public class UrlHelper
    {
        public static string GetCurrentDomain(HttpContext httpContext)
        {
            var host = httpContext.Request.Host.ToUriComponent();

            // Canonical host is always the apex — never www. (www 301-redirects to
            // it at the edge, but strip here too so canonical/og URLs are apex even
            // if a www request ever reaches the app).
            if (host.StartsWith("www.", System.StringComparison.OrdinalIgnoreCase))
            {
                host = host[4..];
            }

            return string.Format(
                "{0}{1}{2}",
                httpContext.Request.Scheme,
                "://",
                host);
        }
    }
}