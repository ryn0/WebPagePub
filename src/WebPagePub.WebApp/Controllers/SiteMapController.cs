using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using WebPagePub.Core;
using WebPagePub.Data.Constants;
using WebPagePub.Data.Models;
using WebPagePub.Data.Models.Db;
using WebPagePub.Data.Models.Transfer;
using WebPagePub.Managers.Interfaces;
using WebPagePub.Services.Interfaces;
using WebPagePub.Web.Helpers;
using WebPagePub.Web.Models;
using WebPagePub.WebApp.Enums;
using WebPagePub.WebApp.Models;

namespace WebPagePub.Web.Controllers
{
    public class SiteMapController : Controller
    {
        private static readonly TimeSpan CacheSlidingExpiry = TimeSpan.FromMinutes(IntegerConstants.PageCachingMinutes);
        private const long DefaultPageEntrySize = 64 * 1024;     // ~64 KB per page model

        private const int MaxPageSizeForSiteMap = 50000;

        private readonly ICacheService cacheService;
        private readonly IMemoryCache memoryCache;
        private readonly ISitePageManager sitePageManager;

        public SiteMapController(
            ISitePageManager sitePageManager,
            ICacheService cacheService,
            IMemoryCache memoryCache)
        {
            this.sitePageManager = sitePageManager;
            this.cacheService = cacheService;
            this.memoryCache = memoryCache;
        }

        [Route("sitemap_index.xml")]
        public IActionResult SiteMapIndex()
        {
            return this.RedirectPermanent("~/sitemap.xml");
        }

        [Route("sitemap.xml")]
        public IActionResult Index()
        {
            var siteMapHelper = new SiteMapHelper();
            string xml;
            var cacheKey = CacheHelper.GetPageCacheKey(
                    nameof(SiteMapController),
                    nameof(this.Index),
                    1,
                    nameof(this.Index));

            var cachedPage = this.memoryCache.Get(cacheKey);

            if (cachedPage == null)
            {
                var allPages = this.sitePageManager.GetLivePage(1, MaxPageSizeForSiteMap, out int total);

                foreach (var page in allPages)
                {
                    if (!page.IsLive || page.ExcludePageFromSiteMapXml)
                    {
                        continue;
                    }

                    this.AddPageToSiteMap(page, siteMapHelper);
                }

                xml = siteMapHelper.GenerateXml();

                var options = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(CacheSlidingExpiry)
                    .SetPriority(CacheItemPriority.Normal)
                    .SetSize(DefaultPageEntrySize);

                this.memoryCache.Set(
                    cacheKey,
                    xml,
                    options);
            }
            else
            {
                xml = (string)cachedPage;
            }

            return this.Content(xml, "text/xml");
        }

        [Route("sitemap")]
        public IActionResult SiteMap()
        {
            var cacheKey = CacheHelper.GetPageCacheKey(
                                nameof(SiteMapController),
                                nameof(this.SiteMap),
                                1,
                                nameof(this.SiteMap));

            var cachedPage = this.memoryCache.Get(cacheKey);
            var model = new HtmlSiteMapModel();

            if (cachedPage == null)
            {
                var sectionsAndPages = this.sitePageManager.GetAllLinksAndTitles();

                model = this.ConvertToHtmlSiteMapModel(sectionsAndPages);

                var options = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(CacheSlidingExpiry)
                    .SetPriority(CacheItemPriority.Normal)
                    .SetSize(DefaultPageEntrySize);

                this.memoryCache.Set(
                    cacheKey,
                    model,
                    options);
            }
            else
            {
                model = (HtmlSiteMapModel)cachedPage;
            }

            return this.View(nameof(this.Index), model);
        }

        private void AddPageToSiteMap(SitePage page, SiteMapHelper siteMapHelper)
        {
            string url;
            if (page.IsSectionHomePage)
            {
                // TODO: prevent duplicate content and pages which can be indexed a different way ex: /blog/ryan-into-travel is the homepage of blog
                var siteSection = this.sitePageManager.GetSiteSection(page.SitePageSectionId);
                if (siteSection.IsHomePageSection)
                {
                    url = new Uri(UrlHelper.GetCurrentDomain(this.HttpContext)).ToString();
                }
                else
                {
                    url = new Uri(string.Format(
                        "{0}/{1}",
                        UrlHelper.GetCurrentDomain(this.HttpContext),
                        siteSection.Key)).ToString();
                }
            }
            else
            {
                url = new Uri(string.Format(
                    "{0}{1}",
                    UrlHelper.GetCurrentDomain(this.HttpContext),
                    UrlBuilder.BlogUrlPath(page.SitePageSection.Key, page.Key))).ToString().TrimEnd('/');
            }

            var lastUpdated = page.UpdateDate == null ? page.CreateDate : (DateTime)page.UpdateDate;

            if (siteMapHelper.SiteMapItems.FirstOrDefault(x => x.Url == url) != null)
            {
                return;
            }

            if (page.PageType == Data.Enums.PageType.Photo)
            {
                var photos = this.sitePageManager.GetBlogPhotos(page.SitePageId).ToList();
                var siteMapPhotoItems = this.ConvertToSiteMapImages(photos).ToList();
                siteMapHelper.AddUrl(
                    url,
                    lastUpdated,
                    ChangeFrequency.Weekly,
                    .5,
                    siteMapPhotoItems);
            }
            else
            {
                siteMapHelper.AddUrl(url, lastUpdated, ChangeFrequency.Weekly, .5, new List<SiteMapImageItem>());
            }
        }

        private List<SiteMapImageItem> ConvertToSiteMapImages(List<SitePagePhoto> photos)
        {
            var mc = new ModelConverter(this.cacheService);

            var items = new List<SiteMapImageItem>();

            foreach (var photo in photos)
            {
                items.Add(new SiteMapImageItem()
                {
                    ImageLocation = UrlBuilder.ConvertBlobToCdnUrl(photo.PhotoFullScreenUrl, mc.BlobPrefix, mc.CdnPrefix),
                    Title = photo.Title,
                    Caption = photo.Description
                });
            }

            return items;
        }

        private void AddPagesToSection(
            HtmlSiteMapModel model,
            SitePageSection section,
            List<SitePage> allPagesInSection,
            SitePage? indexPage)
        {
            if (indexPage == null)
            {
                return;
            }

            var sectionUrl = this.GetSectionUrl(section, indexPage);

            var siteSectionPage =
                new SectionPage()
                {
                    AnchorText = indexPage.BreadcrumbName,
                    CanonicalUrl = sectionUrl
                };

            foreach (var page in allPagesInSection)
            {
                if (!page.IsLive || page.IsSectionHomePage)
                {
                    continue;
                }

                this.AddPagesInSection(siteSectionPage, page);
            }

            siteSectionPage.ChildPages = siteSectionPage.ChildPages.OrderBy(x => x.AnchorText).ToList();

            model.SectionPages.Add(siteSectionPage);
        }

        private void AddPagesInSection(SectionPage siteSectionPage, SitePage page)
        {
            var pagePath = UrlBuilder.BlogUrlPath(page.SitePageSection.Key, page.Key);
            var pageUrl = new Uri(UrlHelper.GetCurrentDomain(this.HttpContext) + pagePath).ToString().TrimEnd('/');

            siteSectionPage.ChildPages.Add(
                new SectionPage()
                {
                    CanonicalUrl = pageUrl,
                    AnchorText = page.BreadcrumbName
                });
        }

        private string GetSectionUrl(SitePageSection section, SitePage indexPage)
        {
            var sectionPath = UrlBuilder.BlogUrlPath(indexPage.SitePageSection.Key, indexPage.Key);

            if (section.IsHomePageSection && indexPage.IsSectionHomePage)
            {
                return new Uri(UrlHelper.GetCurrentDomain(this.HttpContext)).ToString().TrimEnd('/');
            }

            if (indexPage.IsSectionHomePage)
            {
                return new Uri(
                    $"{UrlHelper.GetCurrentDomain(this.HttpContext)}/{indexPage.SitePageSection.Key}").ToString().TrimEnd('/');
            }

            return new Uri(
                $"{UrlHelper.GetCurrentDomain(this.HttpContext)}{sectionPath}").ToString().TrimEnd('/');
        }

        private HtmlSiteMapModel ConvertToHtmlSiteMapModel(IList<SiteMapDisplaySection> sections)
        {
            var htmlSiteMapModel = new HtmlSiteMapModel();

            foreach (var section in sections)
            {
                var sectionPage = new SectionPage
                {
                    CanonicalUrl = string.Format("{0}{1}", UrlHelper.GetCurrentDomain(this.HttpContext), section.RelativePath),
                    AnchorText = section.PageTitle
                };

                foreach (var item in section.Items)
                {
                    var childPage = new SectionPage
                    {
                        CanonicalUrl = string.Format("{0}{1}", UrlHelper.GetCurrentDomain(this.HttpContext), item.RelativePath),
                        AnchorText = item.PageTitle
                    };

                    sectionPage.ChildPages.Add(childPage);
                }

                htmlSiteMapModel.SectionPages.Add(sectionPage);
            }

            return htmlSiteMapModel;
        }
    }
}