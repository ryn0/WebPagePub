using Microsoft.AspNetCore.Http;
using WebPagePub.Data.Constants;
using System;

namespace WebPagePub.Web.Helpers
{
    public class UrlBuilder
    {
        public static string BlogUrlPath(string sectionKey, string pageKey)
        {
            if (sectionKey == StringConstants.HomeSectionKey &&
                pageKey == StringConstants.HomeIndexPageKey)
            {
                return "/";
            }

            if (pageKey == StringConstants.HomeIndexPageKey)
            {
                return string.Format("/{0}", sectionKey);
            }

            return string.Format("/{0}/{1}", sectionKey, pageKey);
        }

        public static string BlogPreviewUrlPath(int sitePageId)
        {
            return string.Format("/preview/{0}",
                sitePageId);
        }

        public static string GetCurrentDomain(HttpContext httpContext)
        {
            return httpContext.Request.Scheme +
                                    "://" +
                                    httpContext.Request.Host.ToUriComponent();
        }
    }
}
