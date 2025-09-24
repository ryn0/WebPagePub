using WebPagePub.Data.Models;

namespace WebPagePub.WebApp.Models.Admin.SearchLogs
{
    public class SiteSearchLogListModel
    {
        // Filters
        public DateTime? FromUtc { get; set; }
        public DateTime? ToUtc { get; set; }
        public string? Term { get; set; }

        // Paging
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public int TotalCount { get; set; }
        public int PageCount => (int)Math.Ceiling((double)this.TotalCount / Math.Max(1, this.PageSize));

        // Data
        public List<SiteSearchLog> Items { get; set; } = new ();
    }
}
