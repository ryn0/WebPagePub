using System;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using WebPagePub.Core;
using WebPagePub.Data.Constants;
using WebPagePub.Data.Enums;
using WebPagePub.Data.Models;
using WebPagePub.Managers.Interfaces;
using WebPagePub.Services.Interfaces;
using WebPagePub.Web.Helpers;

namespace WebPagePub.Web.Controllers
{
    /// <summary>
    /// Serves <c>/llms.txt</c> (the llmstxt.org convention): a plain-markdown map of the site's
    /// live content so LLMs and AI crawlers can find the good pages quickly. Generated per-site
    /// from that site's own config + pages, and cached like the sitemap.
    /// </summary>
    public class LlmsController : Controller
    {
        private static readonly TimeSpan CacheSlidingExpiry = TimeSpan.FromMinutes(IntegerConstants.PageCachingMinutes);
        private const long DefaultPageEntrySize = 64 * 1024;
        private const int MaxPages = 50000;

        private readonly ISitePageManager sitePageManager;
        private readonly ICacheService cacheService;
        private readonly IMemoryCache memoryCache;

        public LlmsController(
            ISitePageManager sitePageManager,
            ICacheService cacheService,
            IMemoryCache memoryCache)
        {
            this.sitePageManager = sitePageManager;
            this.cacheService = cacheService;
            this.memoryCache = memoryCache;
        }

        [Route("llms.txt")]
        public IActionResult Index()
        {
            var cacheKey = CacheHelper.GetPageCacheKey(
                nameof(LlmsController), nameof(this.Index), 1, nameof(this.Index));

            if (this.memoryCache.Get(cacheKey) is string cachedText)
            {
                return this.Content(cachedText, "text/plain; charset=utf-8");
            }

            var text = this.Build();

            this.memoryCache.Set(
                cacheKey,
                text,
                new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(CacheSlidingExpiry)
                    .SetPriority(CacheItemPriority.Normal)
                    .SetSize(DefaultPageEntrySize));

            return this.Content(text, "text/plain; charset=utf-8");
        }

        private string Build()
        {
            var domain = new Uri(UrlHelper.GetCurrentDomain(this.HttpContext)).ToString().TrimEnd('/');

            var siteName = (this.cacheService.GetSnippet(SiteConfigSetting.WebsiteName) ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(siteName))
            {
                siteName = new Uri(domain).Host;
            }

            var pages = this.sitePageManager.GetLivePage(1, MaxPages, out _)
                .Where(p => p.IsLive && !p.ExcludePageFromSiteMapXml && p.SitePageSection != null)
                .ToList();

            var home = pages.FirstOrDefault(p => p.IsSectionHomePage && p.SitePageSection.IsHomePageSection);
            var blurb = Clean(home?.MetaDescription);

            var sb = new StringBuilder();
            sb.Append("# ").Append(siteName).Append('\n').Append('\n');
            if (!string.IsNullOrEmpty(blurb))
            {
                sb.Append("> ").Append(blurb).Append('\n').Append('\n');
            }

            sb.Append(siteName).Append(" — ").Append(domain)
              .Append(". The pages below are this site's live content, grouped by section.")
              .Append('\n').Append('\n');

            // Group by section: home section first, then sections alphabetically. Within a section,
            // its index page first, then pages alphabetically by title.
            var groups = pages
                .GroupBy(p => p.SitePageSection.SitePageSectionId)
                .Select(g => new
                {
                    Section = g.First().SitePageSection,
                    Pages = g
                        .OrderByDescending(p => p.IsSectionHomePage)
                        .ThenBy(p => p.Title, StringComparer.OrdinalIgnoreCase)
                        .ToList(),
                })
                .OrderByDescending(x => x.Section.IsHomePageSection)
                .ThenBy(x => x.Section.Title, StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var grp in groups)
            {
                var heading = Clean(grp.Section.Title);
                if (string.IsNullOrEmpty(heading))
                {
                    heading = grp.Section.IsHomePageSection ? "Main" : Clean(grp.Section.Key);
                }

                sb.Append("## ").Append(heading).Append('\n').Append('\n');

                foreach (var p in grp.Pages)
                {
                    var title = Clean(p.Title);
                    if (string.IsNullOrEmpty(title))
                    {
                        title = Clean(p.PageHeader);
                    }

                    if (string.IsNullOrEmpty(title))
                    {
                        continue;
                    }

                    var desc = Clean(p.MetaDescription);
                    sb.Append("- [").Append(title).Append("](").Append(this.PageUrl(domain, p)).Append(')');
                    if (!string.IsNullOrEmpty(desc))
                    {
                        sb.Append(": ").Append(desc);
                    }

                    sb.Append('\n');
                }

                sb.Append('\n');
            }

            return sb.ToString();
        }

        private string PageUrl(string domain, SitePage page)
        {
            if (page.IsSectionHomePage)
            {
                return page.SitePageSection.IsHomePageSection
                    ? domain
                    : $"{domain}/{page.SitePageSection.Key}";
            }

            var path = $"{domain}{UrlBuilder.BlogUrlPath(page.SitePageSection.Key, page.Key)}";
            return path.TrimEnd('/');
        }

        private static string Clean(string? s) =>
            (s ?? string.Empty).Replace("\r", " ").Replace("\n", " ").Replace("]", ")").Trim();
    }
}
