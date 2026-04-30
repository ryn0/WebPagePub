using System;
using System.Text.RegularExpressions;

namespace WebPagePub.Core.Utilities
{
    public class TextUtilities
    {
        public static int GetWordCount(string html)
        {
            if (string.IsNullOrEmpty(html))
            {
                return 0;
            }

            var text = StripHtml(html);

            // Split on any whitespace (space, tab, \r, \n) and drop empty entries.
            // The previous implementation only replaced "\r\n" and split on ' ',
            // which under-counted on Linux (LF-only line endings) and on text using
            // tabs as separators. Passing a null char[] tells Split to use all
            // Unicode whitespace as separators.
            var words = text.Split(default(char[])!, StringSplitOptions.RemoveEmptyEntries);

            return words.Length;
        }

        public static string StripHtml(string htmlString)
        {
            string pattern = @"<(.|\n)*?>";

            return Regex.Replace(htmlString, pattern, string.Empty);
        }

        public static string RemoveNonAlphaNumeric(string input)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            var result = rgx.Replace(input, string.Empty);
            return result;
        }
    }
}