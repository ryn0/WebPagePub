using Microsoft.AspNetCore.Mvc;
using WebPagePub.Data.Models;
using WebPagePub.Data.Models.Db;
using WebPagePub.Data.Repositories.Interfaces;
using WebPagePub.Web.Helpers;
using WebPagePub.Web.Models;

namespace WebPagePub.Web.Controllers
{
    public class SiteMapController : Controller
    {
        private const int MaxPageSizeForSiteMap = 50000;
        private readonly ISitePageRepository sitePageRepository;
        private readonly ISitePageSectionRepository sitePageSectionRepository;

        public SiteMapController(
            ISitePageRepository sitePageRepository,
            ISitePageSectionRepository sitePageSectionRepository)
        {
           this.sitePageRepository = sitePageRepository;
           this.sitePageSectionRepository = sitePageSectionRepository;
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
                    url = new Uri(UrlBuilder.GetCurrentDomain(this.HttpContext)).ToString();
                }
                else
                {
                    url = new Uri(UrlBuilder.GetCurrentDomain(this.HttpContext) +
                                     UrlBuilder.BlogUrlPath(page.SitePageSection.Key, page.Key)).ToString().TrimEnd('/');
                }

                var lastUpdated = page.UpdateDate == null ? page.CreateDate : (DateTime)page.UpdateDate;
                siteMapHelper.AddUrl(url, lastUpdated, ChangeFrequency.Weekly, .5);
            }

            var xml = siteMapHelper.GenerateXml();

            return this.Content(xml, "text/xml");
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

                    var pagePath = UrlBuilder.BlogUrlPath(page.SitePageSection.Key, page.Key);
                    var pageUrl = new Uri(UrlBuilder.GetCurrentDomain(this.HttpContext) + pagePath).ToString().TrimEnd('/');

                    siteSectionPage.ChildPages.Add(
                        new SectionPage()
                        {
                            CanonicalUrl = pageUrl,
                            AnchorText = page.BreadcrumbName
                        });
                }

                siteSectionPage.ChildPages = siteSectionPage.ChildPages.OrderBy(x => x.AnchorText).ToList();

                model.SectionPages.Add(siteSectionPage);
            }

            model.SectionPages = model.SectionPages.OrderBy(x => x.AnchorText).ToList();

            return this.View(nameof(Index), model);
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