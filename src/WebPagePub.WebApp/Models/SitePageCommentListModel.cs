namespace WebPagePub.Web.Models
{
    public class SitePageCommentListModel
    {
        public int PageCount { get; set; }

        public int Total { get; set; }

        public int CurrentPageNumber { get; set; }

        public int QuantityPerPage { get; set; }

        public List<SitePageCommentItemModel> Items { get; set; } = new List<SitePageCommentItemModel>();
    }
}
