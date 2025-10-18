using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using WebPagePub.Data.Constants;
using WebPagePub.Services.Helpers;
using WebPagePub.Services.Interfaces;

namespace WebPagePub.Services.Implementations
{
    public sealed class SnippetFetcher : ISnippetFetcher
    {
        private const long DefaultSectionEntrySize = 8 * 1024;   // ~8 KB per section (fallback)
        private static readonly TimeSpan CacheSlidingExpiry = TimeSpan.FromMinutes(IntegerConstants.CacheInMinutes);

        private readonly IHttpClientFactory httpFactory;
        private readonly IMemoryCache cache;

        public SnippetFetcher(IHttpClientFactory httpFactory, IMemoryCache cache)
        {
            this.httpFactory = httpFactory;
            this.cache = cache;
        }

        public async Task<string> GetAsync(string url, TimeSpan cacheFor)
        {
            if (this.cache.TryGetValue(url, out string cached))
            {
                return cached;
            }

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var client = this.httpFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(8);

            var html = await client.GetStringAsync(url, cts.Token);

            // Ensure entry has a Size in KB (required for SizeLimit to apply)
            var sizeKb = MemoryCacheSizeEstimator.EstimateKb(html);
            if (sizeKb <= 0) sizeKb = DefaultSectionEntrySize / 1024;

            var options = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(CacheSlidingExpiry)
                .SetAbsoluteExpiration(DateTimeOffset.UtcNow.Add(cacheFor))
                .SetSize(sizeKb);

            this.cache.Set(url, html, options);
            return html;
        }

        public Task<string> GetAsync(string url)
        {
            throw new NotImplementedException();
        }
    }
}