using System.Text.RegularExpressions;

namespace WebPagePub.WebApp.Helpers
{
    public static class LinkChecker
    {
        public static bool ContainsLink(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            // Regular expression pattern to identify URLs
            var urlPattern = @"(http|https):\/\/([A-Za-z0-9\-]+\.)+[A-Za-z]{2,6}([\/A-Za-z0-9\-\._~:\/?#\[\]@!$&'()*+,;=]*)?";

            // Create a Regex object with the pattern
            var regex = new Regex(urlPattern, RegexOptions.IgnoreCase);

            // Check if the text contains a match
            return regex.IsMatch(text);
        }
    }
}