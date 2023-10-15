namespace WebPagePub.WebApp.Models.Reports
{
    public class ClickReportModel
    {
        public int TotalClicks { get; set; }

        public int UniqueIps { get; set; }

        public int TotalBotIps { get; set; }

        public List<ClickReportItemModel> UrlClicks { get; set; } = new List<ClickReportItemModel>();

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}
