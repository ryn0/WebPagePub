using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebPagePub.Web.Models
{
    public class ClickReportModel
    {
        public int TotalClicks { get; set; }

        public int UniqueIps { get; set; }

        public List<UrlClickReportModel> UrlClicks { get; set; } = new List<UrlClickReportModel>();

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }

    public class UrlClickReportModel
    {
        public string Url { get; set; }

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
