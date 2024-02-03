using System.Text;
using WebPagePub.WebApp.Enums;
using WebPagePub.WebApp.Models;

namespace WebPagePub.Web.Helpers
{
    public class SiteMapHelper
    {
        public List<SiteMapItem> SiteMapItems { get; set; } = new List<SiteMapItem>();

        public void AddUrl(string url, DateTime lastMod, ChangeFrequency changeFrequency, double priority, List<SiteMapImageItem> images)
        {
            this.SiteMapItems.Add(new SiteMapItem
            {
                Url = url,
                LastMode = lastMod,
                ChangeFrequency = changeFrequency,
                Priority = priority,
                Images = images
            });
        }

        public string GenerateXml()
        {
            var sb = new StringBuilder();
            sb.AppendLine(@"<?xml version=""1.0"" encoding=""UTF-8""?>");
            sb.AppendLine(@"<urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">");

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
                    foreach (var imageItem in siteMapItem.Images)
                    {
                        sb.AppendLine(@"     <image:image>");
                        sb.AppendFormat(@"         <image:loc>{0}</image:loc>", imageItem.ImageLocation);
                        sb.AppendLine();
                        sb.AppendLine(@"     </image:image>");
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
