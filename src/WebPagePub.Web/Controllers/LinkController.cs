using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using WebPagePub.Data.Repositories.Interfaces;
using System.Threading.Tasks;
using WebPagePub.Data.Models.Db;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System;
using WebPagePub.Data.Constants;
using System.Linq;
using System.Collections.Generic;
using WebPagePub.Web.Helpers;

namespace WebPagePub.Web.Controllers
{
    public class LinkController : Controller
    {
       
        private IMemoryCache _memoryCache;
        private readonly ILinkRedirectionRepository _linkRedirectionRepository;
        private readonly IClickLogRepository _clickLogRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public LinkController(
            IMemoryCache memoryCache, 
            ILinkRedirectionRepository linkRedirectionRepository,
            IClickLogRepository clickLogRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _memoryCache = memoryCache;
            _linkRedirectionRepository = linkRedirectionRepository;
            _clickLogRepository = clickLogRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        [Route("go/{key}")]
        public async Task<ActionResult> Go(string key)
        {
            var url = GetLinkForKey(key);

            if (url == null)
                Redirect("~/");

            await LogClickAsync();

            return Redirect(url);
        }

        private async Task LogClickAsync()
        {
            var userAgent = _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString();

            var headers = GetHeadersString(_httpContextAccessor.HttpContext.Request);

            if (!StringConstants.BotUserAgents.Any(userAgent.Contains))
            {
                await _clickLogRepository.CreateAsync(new ClickLog()
                {
                    IpAddress = HttpContext.Connection.RemoteIpAddress.ToString(),
                    Url = _httpContextAccessor.HttpContext?.Request?.GetDisplayUrl(),
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
   
            return String.Join(Environment.NewLine, requestHeaders.ToArray());   
        }

        private string GetLinkForKey(string key)
        {
            var cacheKey = CacheHelper.GetLinkCacheKey(key);

            if (_memoryCache.TryGetValue(cacheKey, out string destination))
            {
                return destination;
            }
            else
            {
                var link = _linkRedirectionRepository.Get(key);

                _memoryCache.Set(cacheKey, link.UrlDestination);

                return link.UrlDestination;
            }

        }
    }
}
