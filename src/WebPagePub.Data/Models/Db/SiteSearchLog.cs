using WebPagePub.Data.DbModels.BaseDbModels;

namespace WebPagePub.Data.Models
{
    public class SiteSearchLog : CreatedStateInfo
    {
        public int SiteSearchLogId { get; set; }
        public string Term { get; set; } = string.Empty;
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int ResultsCount { get; set; }
        public string? ClientIp { get; set; }
        public string? UserAgent { get; set; }
        public string? Referer { get; set; }
        public string? Path { get; set; }
    }
}