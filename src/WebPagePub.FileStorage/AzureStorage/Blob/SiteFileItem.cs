namespace WebPagePub.FileStorage.AzureStorage.Blob
{
    public class SiteFileItem
    {
        public bool IsFolder { get; set; } = false;

        required public string FolderName { get; set; }

        required public string FolderPathFromRoot { get; set; }

        required public string FilePath { get; set; }
    }
}