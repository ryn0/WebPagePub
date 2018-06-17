using System;
using Microsoft.AspNetCore.Mvc;
using WebPagePub.Data.Repositories.Interfaces;
using WebPagePub.Web.Helpers;
using WebPagePub.Web.Models;
using System.Linq;

namespace WebPagePub.Web.Controllers
{
    public class SiteMapController : Controller
    {
        const int MaxPageSizeForSiteMap = 50000;
        private readonly ISitePageRepository _sitePageRepository;

        public SiteMapController(ISitePageRepository SitePageRepository)
        {
            _sitePageRepository = SitePageRepository;
        }

        [Route("sitemap.xml")]
        public IActionResult Index()
        {
            var allPages = _sitePageRepository.GetLivePage(1, MaxPageSizeForSiteMap, out int total);

            var siteMapHelper = new SiteMapHelper();

            foreach (var page in allPages)
            {
                if (!page.IsLive || page.ExcludePageFromSiteMapXml)
                    continue;

                string url;
                if (page.IsHomePage)
                {
                    url = new Uri(UrlBuilder.GetCurrentDomain(HttpContext)).ToString();
                }
                else
                {
                    url = new Uri(UrlBuilder.GetCurrentDomain(HttpContext) +
                                     UrlBuilder.BlogUrlPath(page.SitePageSection.Key, page.Key)).ToString().TrimEnd('/');
                }

                var lastUpdated = page.UpdateDate == null ? page.CreateDate : (DateTime)page.UpdateDate;
                siteMapHelper.AddUrl(url, lastUpdated, ChangeFrequency.weekly, .5);
            }

            var xml = siteMapHelper.GenerateXml();

            return Content(xml, "text/xml");
        }

        [Route("sitemap")]
        public IActionResult SiteMap()
        {
            var model = new HtmlSiteMapModel();
            var allPages = _sitePageRepository.GetLivePage(1, int.MaxValue, out int total);
            var sectionIds = allPages.Select(x => x.SitePageSectionId).Distinct();

            foreach (var sectionId in sectionIds)
            {
                var allPagesInSection = allPages.Where(x => x.SitePageSectionId == sectionId).ToList();

                var indexPage = allPagesInSection.First(x => x.IsHomePage == true);
                var sectionPath = UrlBuilder.BlogUrlPath(indexPage.SitePageSection.Key, indexPage.Key);
                var sectionUrl = new Uri(UrlBuilder.GetCurrentDomain(HttpContext) + sectionPath).ToString().TrimEnd('/');

                var siteSectionPage =
                    new SectionPage()
                    {
                        AnchorText = indexPage.BreadcrumbName,
                        CanonicalUrl = sectionUrl
                    };

                foreach (var page in allPagesInSection)
                {
                    if (!page.IsLive || page.IsHomePage)
                        continue;

                    var pagePath = UrlBuilder.BlogUrlPath(page.SitePageSection.Key, page.Key);
                    var pageUrl = new Uri(UrlBuilder.GetCurrentDomain(HttpContext) + pagePath).ToString().TrimEnd('/');

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

            return View("Index", model);
        }
    }
}
