namespace WebPagePub.Core.Utilities
{
    public class FileNameUtilities
    {
        public static string CleanFileName(string fileName)
        {
            return fileName.Replace(" ", string.Empty);
        }
    }
}
