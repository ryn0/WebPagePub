using Microsoft.Extensions.Caching.Memory;
using System;
using WebPagePub.Data.Enums;
using WebPagePub.Data.Repositories.Interfaces;
using WebPagePub.Services.Interfaces;

namespace WebPagePub.Services.Implementations
{
    public class CacheService : ICacheService
    {
        private const string SnippetCachePrefix = "snippet-";
        private static readonly TimeSpan SlidingExpiry = TimeSpan.FromMinutes(20);

        private readonly IContentSnippetRepository contentSnippetRepository;
        private readonly IMemoryCache memoryCache;

        public CacheService(
            IMemoryCache memoryCache,
            IContentSnippetRepository contentSnippetRepository)
        {
            this.memoryCache = memoryCache;
            this.contentSnippetRepository = contentSnippetRepository;
        }

        public void ClearSnippetCache(SiteConfigSetting snippetType)
        {
            var cacheKey = BuildCacheKey(snippetType);
            this.memoryCache.Remove(cacheKey);
        }

        public string GetSnippet(SiteConfigSetting snippetType)
        {
            var cacheKey = BuildCacheKey(snippetType);

            if (this.memoryCache.TryGetValue(cacheKey, out string snippet) && snippet != null)
            {
                return snippet;
            }

            var dbModel = this.contentSnippetRepository.Get(snippetType);
            var content = dbModel?.Content ?? string.Empty;

            var options = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(SlidingExpiry)
                // optional absolute cap if you also want a hard stop on lifetime:
                // .SetAbsoluteExpiration(TimeSpan.FromHours(12))
                .SetPriority(CacheItemPriority.Normal)
                .SetSize(EstimateBytes(cacheKey, content)); // REQUIRED when SizeLimit is set

            this.memoryCache.Set(cacheKey, content, options);
            return content;
        }

        private static string BuildCacheKey(SiteConfigSetting snippetType)
            => $"{SnippetCachePrefix}{snippetType}";

        // Rough byte-size estimate so the 200 MB global cap is meaningful.
        // .NET strings are UTF-16 (~2 bytes/char). Add small overhead.
        private static long EstimateBytes(string key, string value)
        {
            long bytes = 64; // entry/header fudge factor
            if (!string.IsNullOrEmpty(key)) bytes += (long)key.Length * 2;
            if (!string.IsNullOrEmpty(value)) bytes += (long)value.Length * 2;
            return bytes;
        }
    }
}
