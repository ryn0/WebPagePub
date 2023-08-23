using WebPagePub.ChatCommander.ChatModels;
using WebPagePub.Core.Utilities;

namespace WebPagePub.ChatCommander.Utilities
{
    public class TextHelpers
    {
        public static string CleanTitle(string articleTitle)
        {
            articleTitle = CleanText(articleTitle);
            articleTitle = articleTitle.Replace("<title>", string.Empty);
            articleTitle = articleTitle.Replace("</title>", string.Empty);
            articleTitle = articleTitle.Replace("&amp;", "&");

            return articleTitle;
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
            newText = newText.Replace("the-url-key-for-the-given-question-would-be", string.Empty);

            return newText;
        }

        public static string CleanText(string text)
        {
            var cleanedText = text.Trim().Replace("\"", string.Empty);

            if (cleanedText.EndsWith("\""))
            {
                cleanedText = cleanedText.Remove(cleanedText.Length - 1, 1);
            }

            return cleanedText;
        }

        public static string CleanH1(string input)
        {
            input = CleanText(input);

            var cleaned = input.Replace("<h1>", string.Empty);
            cleaned = cleaned.Replace("</h1>", string.Empty);

            return cleaned;
        }

    }
}
