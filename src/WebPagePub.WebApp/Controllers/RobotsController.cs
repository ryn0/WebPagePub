using System.Text;
using Microsoft.AspNetCore.Mvc;
using WebPagePub.Core;
using WebPagePub.Data.Repositories.Interfaces;
using WebPagePub.Web.Helpers;

namespace WebPagePub.Web.Controllers
{
    public class RobotsController : Controller
    {
        public RobotsController(ISitePageRepository sitePageRepository)
        {
            this.SitePageRepository = sitePageRepository;
        }

        public ISitePageRepository SitePageRepository { get; private set; }

        [Route("robots.txt")]
        [HttpGet]
        public ContentResult RobotsTxt()
        {
            var sb = new StringBuilder();

            sb.AppendLine("User-agent: *");

            var ignoredPages = this.SitePageRepository.GetIgnoredPages();

            foreach (var ignoredPage in ignoredPages)
            {
                string path;

                if (ignoredPage.SitePageSection == null)
                {
                    path = ignoredPage.Key.ToString().TrimEnd('/');
                }
                else
                {
                    path = UrlBuilder.BlogUrlPath(ignoredPage.SitePageSection.Key, ignoredPage.Key).ToString().TrimEnd('/');
                }

                sb.AppendLine(string.Format("Disallow: /{0}", path));
            }

            sb.AppendLine();

            var siteMapUrl = new Uri(new Uri(UrlHelper.GetCurrentDomain(this.HttpContext)), "sitemap.xml");

            sb.AppendLine($"Sitemap: {siteMapUrl}");

            return this.Content(sb.ToString());
        }
    }
}