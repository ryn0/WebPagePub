using WebPagePub.Data.Enums;
using WebPagePub.Data.Repositories.Interfaces;
using WebPagePub.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace WebPagePub.Services.Implementations
{
    public class CacheService : ICacheService
    {
        const string SnippetCachePrefix = "snippet-";
        IMemoryCache _memoryCache;

        private readonly IContentSnippetRepository _contentSnippetRepository;

        public CacheService(
            IMemoryCache memoryCache,
            IContentSnippetRepository contentSnippetRepository)
        {
            _memoryCache = memoryCache;
            _contentSnippetRepository = contentSnippetRepository;
        }

        public void ClearSnippetCache(SiteConfigSetting snippetType)
        {
            var cacheKey = BuildCacheKey(snippetType);

            _memoryCache.Remove(cacheKey);
        }

        public string GetSnippet(SiteConfigSetting snippetType)
        {
            var cacheKey = BuildCacheKey(snippetType);

            if (_memoryCache.TryGetValue(cacheKey, out string snippet))
            {
                return snippet;
            }
            else
            {
                var dbModel = _contentSnippetRepository.Get(snippetType);

                if (dbModel == null)
                    return string.Empty;

                _memoryCache.Set(cacheKey, dbModel.Content);

                return dbModel.Content;
            }
        }

        private string BuildCacheKey(SiteConfigSetting snippetType)
        {
            return string.Format("{0}{1}", SnippetCachePrefix, snippetType.ToString());
        }
    }
}
