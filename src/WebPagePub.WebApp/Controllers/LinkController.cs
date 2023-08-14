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

            if (url == null)
            {
                this.Redirect("~/");
            }

            await this.LogClickAsync();

            return this.Redirect(url);
        }

        private async Task LogClickAsync()
        {
            var userAgent = this.httpContextAccessor.HttpContext.Request?.Headers[StringConstants.UserAgent].ToString();

            var headers = this.GetHeadersString(this.httpContextAccessor.HttpContext.Request);

            if (!StringConstants.BotUserAgents.Any(userAgent.Contains))
            {
                await this.clickLogRepository.CreateAsync(new ClickLog()
                {
                    IpAddress = this.httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(),
                    Url = this.httpContextAccessor.HttpContext?.Request?.GetDisplayUrl(),
                    Headers = headers
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

            if (this.memoryCache.TryGetValue(cacheKey, out string destination))
            {
                return destination;
            }
            else
            {
                var link = this.linkRedirectionRepository.Get(key);

                this.memoryCache.Set(cacheKey, link.UrlDestination);

                return link.UrlDestination;
            }
        }
    }
}
