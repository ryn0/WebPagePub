namespace WebPagePub.WebApp.Models.SitePage
{
    public class SitePagePagingModel
    {
        public int PageCount { get; set; }

        public int Total { get; set; }

        public int CurrentPageNumber { get; set; }

        public int QuantityPerPage { get; set; }
    }
}
