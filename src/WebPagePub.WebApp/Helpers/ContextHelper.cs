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
            var context = httpContextAccessor.HttpContext;
            if (context == null)
            {
                throw new InvalidOperationException("HttpContext is not available.");
            }

            var request = context.Request;
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