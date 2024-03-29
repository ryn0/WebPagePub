﻿namespace WebPagePub.WebApp.Models.SitePage
{
    public class SitePageItemModel
    {
        public int SitePageId { get; set; }

        public bool IsIndex { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime PublishDateTimeUtc { get; set; }

        public string Title { get; set; } = default!;

        public string Key { get; set; } = default!;

        public bool IsLive { get; set; }

        public string LiveUrlPath { get; set; } = default!;

        public string PreviewUrlPath { get; set; } = default!;

        public bool IsSiteSection { get; set; }

        public int SitePageSectionId { get; set; }
    }
}
