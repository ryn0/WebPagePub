﻿namespace WebPagePub.Web.Helpers
{
    public class UrlHelper
    {
        public static string GetCurrentDomain(HttpContext httpContext)
        {
            return string.Format(
                "{0}{1}{2}",
                httpContext.Request.Scheme,
                "://",
                httpContext.Request.Host.ToUriComponent());
        }
    }
}