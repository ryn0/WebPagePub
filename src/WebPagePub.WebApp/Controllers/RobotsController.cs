﻿using System.Text;
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

            sb.AppendLine($"Allow: /Sitemap: {siteMapUrl}");

            var result = RemoveDuplicateLines(sb);

            return this.Content(result.ToString());
        }

        private static StringBuilder RemoveDuplicateLines(StringBuilder sb)
        {
            // Convert StringBuilder content to a string
            string content = sb.ToString();

            // Split the string into lines
            string[] lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            // Use a HashSet to keep track of unique lines
            HashSet<string> uniqueLines = new HashSet<string>();

            // Rebuild the string without duplicate lines
            StringBuilder result = new StringBuilder();
            foreach (string line in lines)
            {
                if (uniqueLines.Add(line))
                {
                    result.AppendLine(line);
                }
            }

            return result;
        }
    }
}