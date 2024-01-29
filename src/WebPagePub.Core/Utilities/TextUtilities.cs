using System.Linq;
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

            var text = html.Replace("\r\n", " ");
            text = StripHtml(text);
            var words = text.Split(' ').Where(x => !string.IsNullOrWhiteSpace(x));

            return words.Count();
        }

        public static string StripHtml(string htmlString)
        {
            string pattern = @"<(.|\n)*?>";

            return Regex.Replace(htmlString, pattern, string.Empty);
        }
    }
}
