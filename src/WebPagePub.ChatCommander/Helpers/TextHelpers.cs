using System.Text.RegularExpressions;
using System.Web;
using WebPagePub.Core.Utilities;

namespace WebPagePub.ChatCommander.Utilities
{
    public class TextHelpers
    {
        public static string ScriptAndFormFormatting(string text)
        {
            text = text.Trim();

            var newText = text.Replace("&lt;/script>", "</script>");

            newText = AddClassesToButton(newText);

            return newText;
        }

        public static string HtmlDecode(string text)
        {
            var myWriter = new StringWriter();

            HttpUtility.HtmlDecode(text, myWriter);

            var decodeHtml = myWriter.ToString();

            return decodeHtml;
        }

        public static string RemoveNonHtmlTextAtStart(string text)
        {
            var indexOfFirstHtmlChar = text.IndexOf("<");

            if (indexOfFirstHtmlChar == -1)
            {
                return text;
            }

            var extractedText = text.Substring(indexOfFirstHtmlChar, (text.Length - indexOfFirstHtmlChar));

            return extractedText;
        }

        public static string[] GetUniqueLines(string text)
        {
            var linesInFile = text.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var uniqueItems = linesInFile.Distinct();

            return uniqueItems.ToArray();
        }

        public static string CleanTitle(string articleTitle)
        {
            articleTitle = CleanText(articleTitle);
            articleTitle = articleTitle.Replace("<title>", string.Empty);
            articleTitle = articleTitle.Replace("</title>", string.Empty);
            articleTitle = articleTitle.Replace("&amp;", "&");
            articleTitle = articleTitle.Replace("”", string.Empty);
            articleTitle = articleTitle.Replace("<h1>", string.Empty);
            articleTitle = articleTitle.Replace("</h1>", string.Empty);
            articleTitle = articleTitle.Replace("<h2>", string.Empty);
            articleTitle = articleTitle.Replace("</h2>", string.Empty);
            articleTitle = articleTitle.Replace("Title:", string.Empty);
            articleTitle = articleTitle.Replace("title:", string.Empty);
            
            if (articleTitle.StartsWith("'"))
            {
                articleTitle = articleTitle.Remove(0, 1);
            }

            if (articleTitle.EndsWith("'"))
            {
                articleTitle = articleTitle.Remove(articleTitle.Length - 1, 1);
            }

            return articleTitle.Trim();
        }

        public static string AddClassesToButton(string text) {
            text = text.Trim();
            var newText = text.Replace("<button", @"<button class=""btn btn-success"" ");
            return newText;
        }

        public static string ParseBreadcrumb(string input)
        {
            input = CleanText(input);

            var lastBreadcrumb = string.Empty;

            if (input.Contains(">"))
            {
                var parts = input.Split('>');
                if (parts.Length > 0)
                {
                    lastBreadcrumb = parts[parts.Length - 1];
                }
            }

            return CleanText(lastBreadcrumb);
        }

        public static string CleanArticleKey(string articleKey)
        {
            articleKey = CleanText(articleKey);

            var newText = articleKey.Trim().UrlKey();

            return newText;
        }

        public static string? TruncateLongString(string str, int maxLength)
        {
            return str?[0..Math.Min(str.Length, maxLength)];
        }

        public static string CleanText(string text)
        {
            text = text.Trim();
            var cleanedText = text.Trim().Replace("\"", string.Empty);

            if (cleanedText.EndsWith("\""))
            {
                cleanedText = cleanedText.Remove(cleanedText.Length - 1, 1);
            }

            return cleanedText;
        }

        public static string CleanMetaDescription(string text)
        {
            var cleanedText = CleanText(text);

            cleanedText = cleanedText.Replace("Meta Description:", string.Empty);
            cleanedText = cleanedText.Replace("Meta description:", string.Empty);
            cleanedText = cleanedText.Replace("meta description:", string.Empty);

            return cleanedText.Trim();
        }

        public static string CleanH1(string input)
        {
            input = CleanText(input);

            var cleaned = input.Replace("<h1>", string.Empty);
            cleaned = cleaned.Replace("</h1>", string.Empty);

            return cleaned;
        }

        public static string StripHtml(string htmlString)
        {
            string pattern = @"<(.|\n)*?>";

            return Regex.Replace(htmlString, pattern, string.Empty);
        }

        public static string ReduceSpaces(string extractedText)
        {
            extractedText = extractedText.Replace(Environment.NewLine, " ");
            extractedText = extractedText.Replace("\n", " ");
            extractedText = extractedText.Replace("\t", " ");

            RegexOptions options = RegexOptions.None;
            Regex regex = new("[ ]{2,}", options);
            extractedText = regex.Replace(extractedText, " ");

            return extractedText;
        }
    }
}