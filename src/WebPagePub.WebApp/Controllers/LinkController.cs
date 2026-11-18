using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using WebPagePub.Data.Constants;
using WebPagePub.Data.Models.Db;
using WebPagePub.Data.Repositories.Interfaces;
using WebPagePub.Web.Helpers;

namespace WebPagePub.Web.Controllers
{
    public class LinkController : Controller
    {
        private readonly IMemoryCache memoryCache;
        private readonly ILinkRedirectionRepository linkRedirectionRepository;
        private readonly IClickLogRepository clickLogRepository;
        private readonly IHttpContextAccessor httpContextAccessor;

        public LinkController(
            IMemoryCache memoryCache,
            ILinkRedirectionRepository linkRedirectionRepository,
            IClickLogRepository clickLogRepository,
            IHttpContextAccessor contextAccessor)
        {
            this.memoryCache = memoryCache;
            this.linkRedirectionRepository = linkRedirectionRepository;
            this.clickLogRepository = clickLogRepository;
            this.httpContextAccessor = contextAccessor;
        }

        [Route("go/{key}")]
        public async Task<ActionResult> Go(string key)
        {
            var url = this.GetLinkForKey(key);
            this.Response.Headers.Append("X-Robots-Tag", "noindex, nofollow");

            if (string.IsNullOrEmpty(url))
            {
                return this.Redirect("~/");
            }

            await this.LogClickAsync();

            return this.Redirect(url);
        }

        private async Task LogClickAsync()
        {
            var context = this.httpContextAccessor?.HttpContext;
            if (context == null)
            {
                return;
            }

            var request = context.Request;
            if (request == null)
            {
                return;
            }

            var userAgent = request.Headers[StringConstants.UserAgent].ToString();

            if (userAgent != null)
            {
                var url = request.GetDisplayUrl();
                var referrer = context.Request.Headers.Referer.ToString();
                var isBot = StringConstants.BotUserAgents.Any(userAgent.Contains)
                    || !CameFromOneOfOurPages(referrer, request.Host.Host);

                // No visitor IP address and no request headers are stored: the click report only
                // needs which link was clicked, from which page, and whether it looks automated.
                await this.clickLogRepository.CreateAsync(new ClickLog()
                {
                    IpAddress = string.Empty,
                    Url = url,
                    Headers = string.Empty,
                    UserAgent = userAgent,
                    RefererUrl = referrer ?? string.Empty,
                    IsBot = isBot
                });
            }
        }

        /// <summary>
        /// A reader's click arrives from one of this site's own pages. Scanners request the
        /// redirect address directly, or from the search page with probe text, so a click with
        /// no own-site page behind it is counted as automated.
        /// </summary>
        public static bool CameFromOneOfOurPages(string referrer, string ownHost)
        {
            if (string.IsNullOrWhiteSpace(referrer)
                || !Uri.TryCreate(referrer, UriKind.Absolute, out var from)
                || !string.Equals(from.Host, ownHost, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var path = from.AbsolutePath;
            return !path.StartsWith("/search", StringComparison.OrdinalIgnoreCase)
                && !path.StartsWith("/go/", StringComparison.OrdinalIgnoreCase)
                && referrer.IndexOfAny(new[] { '\'', '"', '(', ')' }) < 0;
        }

        private static readonly HashSet<string> SecretHeaders = new (StringComparer.OrdinalIgnoreCase)
        {
            "Cookie", "Set-Cookie", "Authorization", "Proxy-Authorization", "X-Bunny-Origin", "RequestVerificationToken",
        };

        private string GetHeadersString(HttpRequest request)
        {
            var requestHeaders = new List<string>();

            // Never store credentials: a signed-in admin who clicks an outgoing link sends the
            // sign-in cookie with the request, and it used to be saved here in full.
            foreach (var header in request.Headers)
            {
                if (SecretHeaders.Contains(header.Key))
                {
                    continue;
                }

                var value = header.Value.ToString();
                requestHeaders.Add($"{header.Key}:{(value.Length > 500 ? value[..500] : value)}");
            }

            var all = string.Join(Environment.NewLine, requestHeaders.ToArray());
            return all.Length > 4000 ? all[..4000] : all;
        }

        private const long LinkEntryOverheadBytes = 128; // rough object overhead
        private static readonly TimeSpan CacheSlidingExpiry = TimeSpan.FromMinutes(IntegerConstants.PageCachingMinutes);

        private string GetLinkForKey(string key)
    {
        var cacheKey = CacheHelper.GetLinkCacheKey(key);

        if (this.memoryCache.TryGetValue<string>(cacheKey, out var destination) &&
            !string.IsNullOrWhiteSpace(destination))
        {
            return destination;
        }

        var link = this.linkRedirectionRepository.Get(key);

        if (link is null || string.IsNullOrWhiteSpace(link.UrlDestination))
        {
            return string.Empty; // or your default
        }

        var value = link.UrlDestination.Trim();

        // char ≈ 2 bytes; add small overhead so we're consistent with a byte-based SizeLimit
        long approxBytes = LinkEntryOverheadBytes + (value.Length * 2L);

        var options = new MemoryCacheEntryOptions()
            .SetSize(approxBytes) // REQUIRED when SizeLimit is set
            .SetSlidingExpiration(CacheSlidingExpiry); // adjust if you like

        this.memoryCache.Set(cacheKey, value, options);

        return value;
    }
}
}