using Microsoft.AspNetCore.Mvc;
using System.Text;
using WebPagePub.Web.Helpers;
using System;

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
            sb.AppendLine("Disallow: ");
            sb.AppendLine();

            var siteMapUrl = new Uri(new Uri(UrlBuilder.GetCurrentDomain(HttpContext)), "sitemap.xml");

            sb.AppendLine($"Sitemap: {siteMapUrl}");

            return Content(sb.ToString());
        }
    }
}