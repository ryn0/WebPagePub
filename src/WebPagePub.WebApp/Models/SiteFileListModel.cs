using System.Collections.Generic;

namespace WebPagePub.Web.Models
{
    public class SiteFileListModel
    {
        public List<SiteFileItem> FileItems { get; set; } = new List<SiteFileItem>();

        public string ParentDirectory { get; set; }

        public string CurrentDirectory { get; set; }
    }
}