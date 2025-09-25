using System.Text.RegularExpressions;

namespace WebPagePub.Web.Helpers
{
    public static class SiteUtilityHelper
    {
        public static string WebSafeMaker(string key)
        {
            var replaceRegex = Regex.Replace(key, @"[\W_-[#]]+", " ");

            var beforeTrim = replaceRegex.Trim()
                                         .Replace("  ", " ")
                                         .Replace(" ", "-")
                                         .Replace("%", string.Empty)
                                         .ToLowerInvariant();

            if (beforeTrim.EndsWith("#"))
            {
                beforeTrim = beforeTrim.TrimEnd('#');
            }

            if (beforeTrim.StartsWith("#"))
            {
                beforeTrim = beforeTrim.TrimStart('#');
            }

            return beforeTrim;
        }

        // keep if you still use it elsewhere
        public static int RandomNumber(int min, int max)
        {
            var rand = new Random();
            return rand.Next(min, max);
        }
    }
}