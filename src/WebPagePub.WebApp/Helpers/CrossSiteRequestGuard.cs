using Microsoft.AspNetCore.Http;

namespace WebPagePub.Web.Helpers
{
    /// <summary>
    /// Stops other websites from triggering actions with a signed-in administrator's session
    /// (a link, form or image on another site that points at an admin action here).
    /// For a signed-in user it refuses:
    ///  - every request that is not a GET/HEAD and comes from another site, and
    ///  - GET requests from another site to the admin areas (several old admin actions,
    ///    such as deleting a page, photo, file or folder, are plain links).
    /// "From another site" is read from the browser's Sec-Fetch-Site header, or, for browsers
    /// that do not send it, from the Origin / Referer host. Typing an address or using a
    /// bookmark is always allowed.
    /// </summary>
    public static class CrossSiteRequestGuard
    {
        private static readonly string[] AdminAreas =
        {
            "/sitepages", "/sitefilesmanagement", "/commentmanagement", "/authormanagement",
            "/contentsnippetmanagement", "/linkmanagement", "/redirectpathmanagement",
            "/emailsubscriptionmanagement", "/usermanagement", "/admin", "/reports", "/account/changepassword",
        };

        public static bool ShouldRefuse(HttpContext context)
        {
            if (context.User?.Identity?.IsAuthenticated != true)
            {
                return false;
            }

            var request = context.Request;
            var safeMethod = HttpMethods.IsGet(request.Method) || HttpMethods.IsHead(request.Method) || HttpMethods.IsOptions(request.Method);
            var path = request.Path.Value ?? string.Empty;
            var adminArea = AdminAreas.Any(a => path.StartsWith(a, StringComparison.OrdinalIgnoreCase));

            if (safeMethod && !adminArea)
            {
                return false;
            }

            return IsFromAnotherSite(request);
        }

        public static bool IsFromAnotherSite(HttpRequest request)
        {
            var fetchSite = request.Headers["Sec-Fetch-Site"].ToString();
            if (!string.IsNullOrEmpty(fetchSite))
            {
                // same-origin / same-site = our own pages; none = typed address or bookmark.
                return fetchSite.Equals("cross-site", StringComparison.OrdinalIgnoreCase);
            }

            var source = request.Headers["Origin"].ToString();
            if (string.IsNullOrEmpty(source) || source == "null")
            {
                source = request.Headers["Referer"].ToString();
            }

            if (string.IsNullOrEmpty(source))
            {
                // No information at all: allow reads, refuse changes.
                return !(HttpMethods.IsGet(request.Method) || HttpMethods.IsHead(request.Method));
            }

            return !Uri.TryCreate(source, UriKind.Absolute, out var uri)
                || !string.Equals(uri.Host, request.Host.Host, StringComparison.OrdinalIgnoreCase);
        }
    }
}
