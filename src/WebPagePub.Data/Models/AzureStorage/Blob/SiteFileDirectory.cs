using System.Collections.Generic;

namespace WebPagePub.Data.Models.AzureStorage.Blob
{
    public class SiteFileDirectory
    {
        public List<SiteFileItem> FileItems { get; set; } = new List<SiteFileItem>();
    }

    public class SiteFileItem
    {
        public bool IsFolder { get; set; } = false;

        public string FolderName { get; set; }

        public string FolderPathFromRoot { get; set; }

        public string FilePath { get; set; }
    }
}