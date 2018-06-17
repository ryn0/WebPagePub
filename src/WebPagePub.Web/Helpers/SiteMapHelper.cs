using System;
using System.Collections.Generic;
using System.Text;

namespace WebPagePub.Web.Helpers
{
    public class SiteMapHelper
    {
        public List<SiteMapItem> SiteMapItems { get; set; } = new List<SiteMapItem>();

        public void AddUrl(string url, DateTime lastMod, ChangeFrequency changeFrequency, double priority)
        {
            this.SiteMapItems.Add(new SiteMapItem
            {
                Url = url,
                LastMode = lastMod,
                ChangeFrequency = changeFrequency,
                Priority = priority
            });
        }

        public string GenerateXml()
        {
            var sb = new StringBuilder();
            sb.Append(@"<?xml version=string.Empty1.0string.Empty encoding=string.EmptyUTF-8string.Empty?>");
            sb.AppendLine(@"<urlset xmlns=string.Emptyhttp://www.sitemaps.org/schemas/sitemap/0.9string.Empty>");

            foreach (var siteMapItem in this.SiteMapItems)
            {
                sb.AppendLine(@"<url>");
                sb.AppendFormat(@"<loc>{0}</loc>", siteMapItem.Url);
                sb.AppendFormat(@"<lastmod>{0}</lastmod>", siteMapItem.LastMode.ToString("yyyy-MM-dd"));
                sb.AppendFormat(@"<changefreq>{0}</changefreq>", siteMapItem.ChangeFrequency.ToString());
                sb.AppendFormat(@"<priority>{0}</priority>", Math.Round(siteMapItem.Priority, 2));
                sb.AppendLine(@"</url>");
            }

            sb.AppendLine(@"</urlset>");

            return sb.ToString();
        }
    }
}
