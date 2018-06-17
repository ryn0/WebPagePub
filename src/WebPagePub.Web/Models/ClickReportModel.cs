using System;
using System.Collections.Generic;

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
}
