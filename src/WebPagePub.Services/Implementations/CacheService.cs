using Microsoft.Extensions.Caching.Memory;
using WebPagePub.Data.Enums;
using WebPagePub.Data.Repositories.Interfaces;
using WebPagePub.Services.Interfaces;

namespace WebPagePub.Services.Implementations
{
    public class CacheService : ICacheService
    {
        private const string SnippetCachePrefix = "snippet-";
        private readonly IContentSnippetRepository contentSnippetRepository;
        private IMemoryCache memoryCache;

        public CacheService(
            IMemoryCache memoryCache,
            IContentSnippetRepository contentSnippetRepository)
        {
            this.memoryCache = memoryCache;
            this.contentSnippetRepository = contentSnippetRepository;
        }

        public void ClearSnippetCache(SiteConfigSetting snippetType)
        {
            var cacheKey = this.BuildCacheKey(snippetType);

            this.memoryCache.Remove(cacheKey);
        }

        public string GetSnippet(SiteConfigSetting snippetType)
        {
            var cacheKey = this.BuildCacheKey(snippetType);

            if (this.memoryCache.TryGetValue(cacheKey, out string snippet))
            {
                return snippet;
            }
            else
            {
                var dbModel = this.contentSnippetRepository.Get(snippetType);

                if (dbModel == null)
                {
                    return string.Empty;
                }

                this.memoryCache.Set(cacheKey, dbModel.Content);

                return dbModel.Content;
            }
        }

        private string BuildCacheKey(SiteConfigSetting snippetType)
        {
            return string.Format("{0}{1}", SnippetCachePrefix, snippetType.ToString());
        }
    }
}
