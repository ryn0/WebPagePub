using System.Collections.Generic;

namespace WebPagePub.Data.Models.AzureStorage.Blob
{
    public class SiteFileDirectory
    {
        public List<SiteFileItem> FileItems { get; set; } = new List<SiteFileItem>();
    }
}