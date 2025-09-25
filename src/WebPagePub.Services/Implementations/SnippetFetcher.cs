using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using WebPagePub.Data.Constants;
using WebPagePub.Services.Interfaces;

public sealed class SnippetFetcher : ISnippetFetcher
{
    private const long DefaultSectionEntrySize = 8 * 1024;   // ~8 KB per section
    private static readonly TimeSpan CacheSlidingExpiry = TimeSpan.FromMinutes(IntegerConstants.PageCachingMinutes);

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

        var client = this.httpFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(5);
        var html = await client.GetStringAsync(url);

        var options = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(CacheSlidingExpiry)
                .SetPriority(CacheItemPriority.Normal)
                .SetSize(DefaultSectionEntrySize);

        this.cache.Set(url, html, options);
        return html;
    }
}