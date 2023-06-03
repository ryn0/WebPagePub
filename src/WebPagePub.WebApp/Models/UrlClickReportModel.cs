using System.Collections.Generic;
using System.Linq;

namespace WebPagePub.Web.Models
{
    public class UrlClickReportModel
    {
        public string Url { get; set; }

        public int TotalClicks { get; set; }

        public List<string> IpsForClick { get; set; } = new List<string>();

        public int UniqueIps
        {
            get
            {
                return this.IpsForClick.Distinct().Count();
            }
        }
    }
}
