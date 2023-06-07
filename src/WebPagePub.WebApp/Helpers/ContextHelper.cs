namespace WebPagePub.Web.Helpers
{
    public static class ContextHelper
    {
        private static IHttpContextAccessor httpContextAccessor;

        public static void Configure(IHttpContextAccessor httpContextAccessor)
        {
            ContextHelper.httpContextAccessor = httpContextAccessor;
        }

        public static Uri GetAbsoluteUri()
        {
            var request = httpContextAccessor.HttpContext.Request;
            UriBuilder uriBuilder = new()
            {
                Scheme = request.Scheme,
                Host = request.Host.ToString(),
                Path = request.Path.ToString(),
                Query = request.QueryString.ToString()
            };
            return uriBuilder.Uri;
        }
    }
}