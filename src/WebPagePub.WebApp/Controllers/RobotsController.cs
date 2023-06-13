using System.Text;
using Microsoft.AspNetCore.Mvc;
using WebPagePub.Web.Helpers;

namespace WebPagePub.Web.Controllers
{
    public class RobotsController : Controller
    {
        public RobotsController()
        {
        }

        [Route("robots.txt")]
        [HttpGet]
        public ContentResult RobotsTxt()
        {
            var sb = new StringBuilder();

            sb.AppendLine("User-agent: *");
            sb.AppendLine("Disallow: /go/*");
            sb.AppendLine();

            var siteMapUrl = new Uri(new Uri(UrlBuilder.GetCurrentDomain(this.HttpContext)), "sitemap.xml");

            sb.AppendLine($"Sitemap: {siteMapUrl}");

            return this.Content(sb.ToString());
        }
    }
}