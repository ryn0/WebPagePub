namespace WebPagePub.WebApp.Models.Reports
{
    public class ReferrerReportModel
    {
        public int TotalClicks { get; set; }

        public int UniqueIps { get; set; }

        public List<ReferrerReportItemModel> UrlClicks { get; set; } = new List<ReferrerReportItemModel>();

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}
