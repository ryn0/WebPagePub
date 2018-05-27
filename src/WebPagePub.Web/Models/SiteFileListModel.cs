using System.Collections.Generic;

namespace WebPagePub.Web.Models
{

    public class SiteFileListModel
    {
        public List<SiteFileItem> FileItems { get; set; } = new List<SiteFileItem>();

        public string ParentDirectory { get; set; }

        public string CurrentDirectory { get; set; }
    }

    public class SiteFileItem
    {
        public bool IsFolder { get; set; } = false;

        public string FolderName { get; set; }

        public string FolderPathFromRoot { get; set; }

        public string FilePath { get; set; }
    }
}