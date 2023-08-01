using System.Text;
using Microsoft.AspNetCore.Mvc;
using WebPagePub.Data.Repositories.Interfaces;
using WebPagePub.Web.Helpers;

namespace WebPagePub.Web.Controllers
{
    public class RobotsController : Controller
    {
        public RobotsController(ISitePageRepository sitePageRepository)
        {
            this.sitePageRepository = sitePageRepository;
        }

        public ISitePageRepository sitePageRepository { get; private set; }

        [Route("robots.txt")]
        [HttpGet]
        public ContentResult RobotsTxt()
        {
            var sb = new StringBuilder();

            sb.AppendLine("User-agent: *");
            sb.AppendLine("Disallow: /go/");
            sb.AppendLine("Disallow: /tag/");
            sb.AppendLine("Disallow: /tags/");

            var ignoredPages = sitePageRepository.GetIgnoredPages();

            foreach (var ignoredPage in ignoredPages)
            {
                string path;

                if (ignoredPage.SitePageSection == null )
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

            var siteMapUrl = new Uri(new Uri(UrlBuilder.GetCurrentDomain(this.HttpContext)), "sitemap.xml");

            sb.AppendLine($"Sitemap: {siteMapUrl}");

            return this.Content(sb.ToString());
        }
    }
}