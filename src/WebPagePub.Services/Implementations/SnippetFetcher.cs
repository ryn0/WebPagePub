using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using WebPagePub.Data.Constants;
using WebPagePub.Services.Interfaces;

public sealed class SnippetFetcher : ISnippetFetcher
{
    private const long DefaultSectionEntrySize = 8 * 1024; // ~8 KB per section
    private static readonly TimeSpan CacheAbsoluteExpiry = TimeSpan.FromMinutes(IntegerConstants.CacheInMinutes);

    private readonly IHttpClientFactory httpFactory;
    private readonly IMemoryCache cache;

    public SnippetFetcher(IHttpClientFactory httpFactory, IMemoryCache cache)
    {
        this.httpFactory = httpFactory;
        this.cache = cache;
    }

    /// <summary>
    /// Always caches for 20 minutes (absolute). After 20 minutes the next call refetches.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<string> GetAsync(string url)
    {
        // Use GetOrCreateAsync so concurrent callers share the same fetch on a miss.
        return await this.cache.GetOrCreateAsync(url, async entry =>
        {
            entry.SetAbsoluteExpiration(CacheAbsoluteExpiry)
                 .SetPriority(CacheItemPriority.Normal)
                 .SetSize(DefaultSectionEntrySize);

            var client = this.httpFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(5);

            return await client.GetStringAsync(url);
        });
    }
}