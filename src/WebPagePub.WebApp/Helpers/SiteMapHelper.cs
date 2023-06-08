﻿using System.Text;

namespace WebPagePub.Web.Helpers
{
    public class SiteMapHelper
    {
        public List<SiteMapItem> SiteMapItems { get; set; } = new List<SiteMapItem>();

        public void AddUrl(string url, DateTime lastMod, ChangeFrequency changeFrequency, double priority, List<string> imageUrls)
        {
            this.SiteMapItems.Add(new SiteMapItem
            {
                Url = url,
                LastMode = lastMod,
                ChangeFrequency = changeFrequency,
                Priority = priority,
                ImageUrls = imageUrls
            });
        }

        public string GenerateXml()
        {
            var sb = new StringBuilder();
            sb.Append(@"<?xml version=""1.0"" encoding=""UTF-8""?>");
            sb.AppendLine(@"<urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"" xmlns:image=""http://www.google.com/schemas/sitemap-image/1.1"">");

            foreach (var siteMapItem in this.SiteMapItems)
            {
                sb.AppendLine(@"<url>");
                sb.AppendFormat(@"<loc>{0}</loc>", siteMapItem.Url);
                sb.AppendFormat(@"<lastmod>{0}</lastmod>", siteMapItem.LastMode.ToString("yyyy-MM-dd"));
                sb.AppendFormat(@"<changefreq>{0}</changefreq>", siteMapItem.ChangeFrequency.ToString());
                sb.AppendFormat(@"<priority>{0}</priority>", Math.Round(siteMapItem.Priority, 2));

                if (siteMapItem.HasImage())
                {
                    sb.AppendLine();
                    foreach (var imageItem in siteMapItem.ImageUrls)
                    {
                        sb.AppendLine(@"<image:image>");
                        sb.AppendFormat(@"<image:loc>{0}</image:loc>", imageItem);
                        sb.AppendLine();
                        sb.AppendLine(@"</image:image>");
                    }
                }
                sb.AppendLine();
                sb.AppendLine(@"</url>");
            }

            sb.AppendLine(@"</urlset>");

            return sb.ToString();
        }
    }
}
