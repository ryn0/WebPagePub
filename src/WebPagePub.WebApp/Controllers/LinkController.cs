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

            if (string.IsNullOrEmpty(url))
            {
                Response.Headers.Add("X-Robots-Tag", "noindex, nofollow");
                return this.Redirect("~/");
            }

            Response.Headers.Add("X-Robots-Tag", "noindex, nofollow");

            await this.LogClickAsync();

            return this.Redirect(url);
        }

        private async Task LogClickAsync()
        {
            var context = this.httpContextAccessor?.HttpContext;
            if (context == null) return;

            var request = context.Request;
            if (request == null) return;

            var userAgent = request.Headers[StringConstants.UserAgent].ToString();
            var headers = this.GetHeadersString(request);

            if (userAgent != null && !StringConstants.BotUserAgents.Any(bot => userAgent.Contains(bot)))
            {
                var ipAddress = context.Connection?.RemoteIpAddress?.ToString();
                var url = request.GetDisplayUrl();
                var referrer = HttpContext.Request.Headers["Referer"].ToString();

                await this.clickLogRepository.CreateAsync(new ClickLog()
                {
                    IpAddress = ipAddress,
                    Url = url,
                    Headers = headers,
                    UserAgent = userAgent,
                    RefererUrl =  referrer ?? string.Empty
                });
            }
        }

        private string GetHeadersString(HttpRequest request)
        {
            var requestHeaders = new List<string>();

            foreach (var header in request.Headers)
            {
                requestHeaders.Add($"{header.Key}:{header.Value}");
            }

            return string.Join(Environment.NewLine, requestHeaders.ToArray());
        }

        private string GetLinkForKey(string key)
        {
            var cacheKey = CacheHelper.GetLinkCacheKey(key);

            if (this.memoryCache.TryGetValue(cacheKey, out string? destination) && !string.IsNullOrEmpty(destination))
            {
                return destination;
            }
            else
            {
                var link = this.linkRedirectionRepository.Get(key);

                if (link == null || string.IsNullOrEmpty(link.UrlDestination))
                {
                    return string.Empty; // or any other default value you'd want to return
                }

                this.memoryCache.Set(cacheKey, link.UrlDestination);

                return link.UrlDestination;
            }
        }
    }
}
