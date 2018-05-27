using WebPagePub.Data.Constants;
using WebPagePub.Data.Models.Db;

namespace WebPagePub.Web.Helpers
{
    public class CacheHelper
    { 
        public static string GetpPageCacheKey(
                                string sectionKey = null,
                                string pageKey = null,
                                bool isPreview = false,
                                int pageNumber = 1,
                                string tagKey = null)
        {
            if (StringConstants.HomeSectionKey == sectionKey &&
                StringConstants.HomeIndexPageKey == pageKey)
                return StringConstants.HomeSectionKey;

            var cacheKey = $"{tagKey}/{sectionKey}/{pageKey}/{pageNumber}".ToLower();

            return cacheKey;
        }

        public static string GetpPageCacheKey(SitePageSection sitePageSection)
        {
            var cacheKey = $"sitepagesection-{sitePageSection.Key}".ToLower();

            return cacheKey;
        }

        public static string GetLinkCacheKey(string key)
        {
            var cacheKey = $"link-{key}".ToLower();

            return cacheKey;
        }
    }
}
