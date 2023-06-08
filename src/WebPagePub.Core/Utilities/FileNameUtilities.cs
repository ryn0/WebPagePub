namespace WebPagePub.Core.Utilities
{
    public class FileNameUtilities
    {
        public static string RemoveSpacesInFileName(string fileName)
        {
            return fileName.Replace(" ", string.Empty);
        }
    }
}
