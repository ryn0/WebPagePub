using System.IO;

namespace WebPagePub.Core.Utilities
{
    public class FileNameUtilities
    {
        public static string RemoveSpacesInFileName(string fileName)
        {
            return fileName.Replace(" ", string.Empty);
        }

        public static string GetFileExtensionLower(string fileName)
        {
            var extension = System.IO.Path.GetExtension(fileName);
            return extension.ToLowerInvariant();
        }

        public static string ChangeFilename(string filepath, string newFilename)
        {
            string dir = Path.GetDirectoryName(filepath);    // @"photo\myFolder"
            string ext = Path.GetExtension(filepath);        // @".jpg"

            return Path.Combine(dir, newFilename + ext); // @"photo\myFolder\image-resize.jpg"
        }
    }
}