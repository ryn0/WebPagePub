namespace WebPagePub.WebApp.Models.Reports
{
    public class ReferrerReportItemModel
    {
        public string UrlRerrer { get; set; } = default!;

        public int TotalClicks { get; set; }

        public List<string> IpsForClick { get; set; } = new List<string>();

        public int UniqueIps
        {
            get
            {
                return IpsForClick.Distinct().Count();
            }
        }
    }
}
