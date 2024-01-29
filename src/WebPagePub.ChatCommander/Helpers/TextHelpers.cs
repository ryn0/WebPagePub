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

            var extractedText = text[indexOfFirstHtmlChar..];

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

        public static string FindWithExactCasing(string input, string searchText)
        {
            Match match = Regex.Match(input, Regex.Escape(searchText), RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Value;
            }
            return string.Empty;
        }

        public static string AddClassesToButton(string text)
        {
            text = text.Trim();
            var newText = text.Replace("<button", @"<button class=""btn btn-success"" ");
            return newText;
        }

        public static string FindAndReplace(string input, string findText, string replaceText)
        {
            return input.Replace(findText, replaceText);
        }

        public static string CaseInsensitiveReplace(string input, string findText, string replaceText)
        {
            return Regex.Replace(input, Regex.Escape(findText), replaceText, RegexOptions.IgnoreCase);
        }

        public static string ParseBreadcrumb(string input)
        {
            input = CleanText(input);

            var lastBreadcrumb = string.Empty;

            if (input.Contains('>'))
            {
                var parts = input.Split('>');
                if (parts.Length > 0)
                {
                    lastBreadcrumb = parts[^1];
                }

                return CleanText(lastBreadcrumb);
            }
            else
            {
                return CleanText(input);
            }
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

        public static bool IsTextSurroundedByPTag(string input, string text)
        {
            if (input == null || text == null)
            {
                return false;
            }

            string pattern = $@"<p>[^<]*{Regex.Escape(input)}[^<]*</p>";
            return Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase);
        }

        public static bool IsTextSurroundedByLiTag(string input, string text)
        {
            if (input == null || text == null)
            {
                return false;
            }

            string pattern = $@"<li>[^<]*{Regex.Escape(input)}[^<]*</li>";
            return Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase);
        }

        public static string InsertLinkInHtml(string htmlContent, string context, string termToLink, string linkTemplate)
        {
            // If the term is not in the context or the context is not in the htmlContent, return the original content.
            if (!context.Contains(termToLink, StringComparison.OrdinalIgnoreCase) ||
                !htmlContent.Contains(context, StringComparison.Ordinal))
            {
                return htmlContent;
            }

            // We'll use the context to locate the exact position of the term within the htmlContent
            int contextStart = htmlContent.IndexOf(context, StringComparison.Ordinal);
            int termStartWithinContext = context.IndexOf(termToLink, StringComparison.OrdinalIgnoreCase);

            // Determine the start index of the term within the htmlContent
            int termStartInHtml = contextStart + termStartWithinContext;

            // Extract the term with its original casing from the htmlContent
            string actualTerm = htmlContent.Substring(termStartInHtml, termToLink.Length);

            // Extract the href value from the linkTemplate
            Match linkMatch = Regex.Match(linkTemplate, @"<a href=""([^""]*)""[^>]*>([^<]*)</a>", RegexOptions.IgnoreCase);
            if (!linkMatch.Success)
                return htmlContent;

            string hrefValue = linkMatch.Groups[1].Value;

            // Construct the final link using the actualTerm
            string finalLink = $@"<a href=""{hrefValue}"">{actualTerm}</a>";

            // Replace the actualTerm with the final link within the htmlContent
            return htmlContent.Substring(0, termStartInHtml) + finalLink + htmlContent.Substring(termStartInHtml + actualTerm.Length);
        }
    }
}