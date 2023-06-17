using Microsoft.AspNetCore.Mvc;
using WebPagePub.Data.Models;
using WebPagePub.Data.Models.Db;
using WebPagePub.Data.Repositories.Interfaces;
using WebPagePub.Services.Interfaces;
using WebPagePub.Web.Helpers;
using WebPagePub.Web.Models;
using WebPagePub.WebApp.Models;

namespace WebPagePub.Web.Controllers
{
    public class SiteMapController : Controller
    {
        private const int MaxPageSizeForSiteMap = 50000;
        private readonly ISitePageRepository sitePageRepository;
        private readonly ISitePagePhotoRepository sitePagePhotoRepository;
        private readonly ISitePageSectionRepository sitePageSectionRepository;
        private readonly ICacheService cacheService;

        public SiteMapController(
            ISitePageRepository sitePageRepository,
            ISitePagePhotoRepository sitePagePhotoRepository,
            ISitePageSectionRepository sitePageSectionRepository,
            ICacheService cacheService)
        {
            this.sitePageRepository = sitePageRepository;
            this.sitePagePhotoRepository = sitePagePhotoRepository;
            this.sitePageSectionRepository = sitePageSectionRepository;
            this.cacheService = cacheService;
        }

        [Route("sitemap.xml")]
        public IActionResult Index()
        {
            var allPages = this.sitePageRepository.GetLivePage(1, MaxPageSizeForSiteMap, out int total);

            var siteMapHelper = new SiteMapHelper();

            foreach (var page in allPages)
            {
                if (!page.IsLive || page.ExcludePageFromSiteMapXml)
                {
                    continue;
                }

                string url;
                if (page.IsSectionHomePage)
                {
                    // TODO: prevent duplicate content and pages which can be indexed a different way ex: /blog/ryan-into-travel is the homepage of blog
                    url = new Uri(UrlBuilder.GetCurrentDomain(this.HttpContext)).ToString();
                }
                else
                {
                    url = new Uri(string.Format("{0}{1}",
                                     UrlBuilder.GetCurrentDomain(this.HttpContext),
                                     UrlBuilder.BlogUrlPath(page.SitePageSection.Key, page.Key))).ToString().TrimEnd('/');
                }

                var lastUpdated = page.UpdateDate == null ? page.CreateDate : (DateTime)page.UpdateDate;

                if (siteMapHelper.SiteMapItems.FirstOrDefault(x => x.Url == url) != null)
                {
                    continue;
                }

                if (page.PageType == Data.Enums.PageType.Photo)
                {
                    var photos = this.sitePagePhotoRepository.GetBlogPhotos(page.SitePageId);
                    var siteMapPhotoItems = ConvertToSiteMapImages(photos);
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

            var xml = siteMapHelper.GenerateXml();

            return this.Content(xml, "text/xml");
        }

        private List<SiteMapImageItem> ConvertToSiteMapImages(List<SitePagePhoto> photos)
        {
            var mc = new ModelConverter(cacheService);

            var items = new List<SiteMapImageItem>();

            foreach (var photo in photos)
            {
                items.Add(new SiteMapImageItem()
                {
                    ImageLocation = mc.ConvertBlobToCdnUrl(photo.PhotoFullScreenUrl),
                    Title = photo.Title,
                    Caption = photo.Description
                });
            }

            return items;
        }

        [Route("sitemap")]
        public IActionResult SiteMap()
        {
            var model = new HtmlSiteMapModel();
            var allPages = this.sitePageRepository.GetLivePage(1, int.MaxValue, out int total);
            var sectionIds = allPages.Select(x => x.SitePageSectionId).Distinct();

            foreach (var sectionId in sectionIds)
            {
                var section = this.sitePageSectionRepository.Get(sectionId);
                var allPagesInSection = allPages.Where(x => x.SitePageSectionId == sectionId).ToList();
                var indexPage = allPagesInSection.FirstOrDefault(x => x.IsSectionHomePage == true);

                if (indexPage == null)
                {
                    continue;
                }

                AddPagesToSection(model, section, allPagesInSection, indexPage);
            }

            model.SectionPages = model.SectionPages.OrderBy(x => x.AnchorText).ToList();

            return this.View(nameof(Index), model);
        }

        private void AddPagesToSection(
            HtmlSiteMapModel model,
            SitePageSection section,
            List<SitePage> allPagesInSection,
            SitePage? indexPage)
        {
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

                AddPagesInSection(siteSectionPage, page);
            }

            siteSectionPage.ChildPages = siteSectionPage.ChildPages.OrderBy(x => x.AnchorText).ToList();

            model.SectionPages.Add(siteSectionPage);
        }

        private void AddPagesInSection(SectionPage siteSectionPage, SitePage page)
        {
            var pagePath = UrlBuilder.BlogUrlPath(page.SitePageSection.Key, page.Key);
            var pageUrl = new Uri(UrlBuilder.GetCurrentDomain(this.HttpContext) + pagePath).ToString().TrimEnd('/');

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
                return new Uri(UrlBuilder.GetCurrentDomain(this.HttpContext)).ToString().TrimEnd('/');
            }

            if (indexPage.IsSectionHomePage)
            {
                return new Uri(
                    $"{UrlBuilder.GetCurrentDomain(this.HttpContext)}/{indexPage.SitePageSection.Key}").ToString().TrimEnd('/');
            }

            return new Uri(
                $"{UrlBuilder.GetCurrentDomain(this.HttpContext)}{sectionPath}").ToString().TrimEnd('/');
        }
    }
}