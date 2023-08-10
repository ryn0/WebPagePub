namespace WebPagePub.WebApp.Models.SitePage
{
    public class SitePageListModel
    {
        public bool IsSiteSectionPage { get; set; }

        public int SitePageSectionId { get; set; }

        public string SitePageSectionTitle { get; set; }

        public int PageCount { get; set; }

        public int Total { get; set; }

        public int CurrentPageNumber { get; set; }

        public int QuantityPerPage { get; set; }

        public List<SitePageItemModel> Items { get; set; } = new List<SitePageItemModel>();
    }
}
