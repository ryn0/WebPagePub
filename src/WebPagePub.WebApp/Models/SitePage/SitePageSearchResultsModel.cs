﻿namespace WebPagePub.WebApp.Models.SitePage
{
    public class SitePageSearchResultsModel
    {
        public bool IsSiteSectionPage { get; set; }

        public string SearchTerm { get; set; } = string.Empty;

        public int PageCount { get; set; }

        public int Total { get; set; }

        public int CurrentPageNumber { get; set; }

        public int QuantityPerPage { get; set; }

        public List<SitePageItemModel> Items { get; set; } = new List<SitePageItemModel>();
    }
}
