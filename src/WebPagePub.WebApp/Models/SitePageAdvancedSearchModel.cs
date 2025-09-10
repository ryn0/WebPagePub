using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebPagePub.WebApp.Models.SitePage
{
    public class SitePageAdvancedSearchModel
    {
        // inputs
        public string? Term { get; set; }
        public string? TagsCsv { get; set; } // comma-separated
        public int? SitePageSectionId { get; set; }
        public bool? IsLive { get; set; }
        public DateTime? PublishedFromUtc { get; set; }
        public DateTime? PublishedToUtc { get; set; }

        // paging
        public int PageNumber { get; set; } = 1;
        public int QuantityPerPage { get; set; } = 10;
        public int Total { get; set; }
        public int PageCount => (int)Math.Ceiling((double) this.Total / Math.Max(1, this.QuantityPerPage));

        // data
        public List<SelectListItem> Sections { get; set; } = new ();
        public List<ResultItem> Items { get; set; } = new ();

        public class ResultItem
        {
            public int SitePageId { get; set; }
            public string Title { get; set; } = string.Empty;
            public string Section { get; set; } = string.Empty;
            public string Key { get; set; } = string.Empty;
            public bool IsLive { get; set; }
            public DateTime? PublishDateTimeUtc { get; set; }
            public string LiveUrlPath { get; set; } = string.Empty;
            public List<string> Tags { get; set; } = new ();
        }
    }
}
