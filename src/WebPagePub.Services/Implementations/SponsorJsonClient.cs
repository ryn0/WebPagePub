using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WebPagePub.Data.Enums;
using WebPagePub.Services.Interfaces;
using WebPagePub.Services.Models.Sponsors;

namespace WebPagePub.Services.Implementations
{

    public class SponsorJsonClient : ISponsorJsonClient
    {
        private const string CacheKey = "SponsorJsonClient.ActiveSponsors";
        private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan HttpTimeout = TimeSpan.FromSeconds(8);

        private readonly IHttpClientFactory httpClientFactory;
        private readonly IMemoryCache memoryCache;
        private readonly ICacheService cacheService;

        public SponsorJsonClient(
            IHttpClientFactory httpClientFactory,
            IMemoryCache memoryCache,
            ICacheService cacheService)
        {
            this.httpClientFactory = httpClientFactory;
            this.memoryCache = memoryCache;
            this.cacheService = cacheService;
        }

        public async Task<IReadOnlyList<SponsorCardItem>> GetActiveSponsorsAsync(CancellationToken ct = default)
        {
            if (this.memoryCache.TryGetValue(CacheKey, out IReadOnlyList<SponsorCardItem>? cached) && cached != null)
            {
                return cached;
            }

            IReadOnlyList<SponsorCardItem> result;

            var rawUrl = (this.cacheService.GetSnippet(SiteConfigSetting.AdNetworkProviderUrl) ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(rawUrl) || !Uri.TryCreate(rawUrl, UriKind.Absolute, out _))
            {
                // No URL configured — cache empty briefly so we don't hammer config lookups
                result = Array.Empty<SponsorCardItem>();
                this.CacheResult(result);
                return result;
            }

            try
            {
                var client = this.httpClientFactory.CreateClient();
                client.Timeout = HttpTimeout;

                var json = await client.GetStringAsync(rawUrl, ct);
                var items = JsonSerializer.Deserialize<List<SponsorCardItem>>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                result = items ?? new List<SponsorCardItem>();
            }
            catch
            {
                // Fail soft — never break the page render because sponsors couldn't be fetched
                result = Array.Empty<SponsorCardItem>();
            }

            this.CacheResult(result);
            return result;
        }

        public async Task<IReadOnlyList<SponsorCardItem>> GetMainSponsorsAsync(CancellationToken ct = default)
        {
            var all = await this.GetActiveSponsorsAsync(ct);
            return all
                .Where(s => s.IsMainSponsor)
                .OrderByDescending(s => s.ExpirationDate)
                .ThenByDescending(s => s.ReviewRating)
                .ThenBy(s => s.Name)
                .ToList();
        }

        private void CacheResult(IReadOnlyList<SponsorCardItem> result)
        {
            var options = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(CacheTtl)
                .SetPriority(CacheItemPriority.Normal)
                .SetSize(8 * 1024); // ~8 KB sizing matches your existing section cache budget

            this.memoryCache.Set(CacheKey, result, options);
        }
    }
}
