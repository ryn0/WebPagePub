using WebPagePub.ChatCommander.Utilities;

namespace WebPagePub.ChatCommander.UnitTests.HelpersTests
{
    public class TextHelpersTests
    {
        [Theory]
        [InlineData("'This is a title'", "This is a title")]
        [InlineData("Title: This is a title", "This is a title")]
        [InlineData("title: This is a title", "This is a title")]
        [InlineData("<h1>This is a title</h1>", "This is a title")]
        [InlineData("<h2>This is a title</h2>", "This is a title")]
        [InlineData("This is a title &amp; something", "This is a title & something")]
        [InlineData("<title>This is a title</title>", "This is a title")]
        [InlineData(@"""This is a title""", "This is a title")]
        [InlineData(@"This ”is” a title", "This is a title")]
        public void CleanTitle_ValidTitle_IsRemovedOfUnwantedCharacters(string input, string expected)
        {
            var result = TextHelpers.CleanTitle(input);

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("Meta Description: This is a meta description.", "This is a meta description.")]
        [InlineData("Meta description: This is a meta description.", "This is a meta description.")]
        [InlineData("meta description: This is a meta description.", "This is a meta description.")]
        public void CleanMetaDescription_ValidDescription_IsRemovedOfUnwantedCharacters(string input, string expected)
        {
            var result = TextHelpers.CleanMetaDescription(input);

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("Sports > Ice Sports > Hockey", "Hockey")]
        [InlineData("States > Florida > Miami", "Miami")]
        [InlineData("Trees > Palm Trees", "Palm Trees")]
        [InlineData("Earth > USA > California > San Diego", "San Diego")]
        [InlineData("San Diego", "San Diego")]
        public void ParseBreadcrumb_ValidDescription_IsRemovedOfUnwantedCharacters(string input, string expected)
        {
            var result = TextHelpers.ParseBreadcrumb(input);

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("<p>This is something.</p>", "This is something.")]
        [InlineData("This is something.", "This is something.")]
        public void StripHtml_ValidText_IsRemovedOfUnwantedCharacters(string input, string expected)
        {
            var result = TextHelpers.StripHtml(input);

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("&lt;div&gt;Hello &amp; World&lt;/div&gt;", "<div>Hello & World</div>")]
        [InlineData("&lt;p&gt;This is a paragraph.&lt;/p&gt;", "<p>This is a paragraph.</p>")]
        [InlineData("No special characters.", "No special characters.")]
        [InlineData("&lt;", "<")]
        public void HtmlDecode_StringsToParse_DecodesHtmlEntitiesCorrectly(string input, string expected)
        {
            // Act
            var result = TextHelpers.HtmlDecode(input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("<div>Hello</div>", "Hello")]
        [InlineData("<div><p>Hello & World</p></div>", "Hello & World")]
        [InlineData("<strong>Bold</strong> and <em>italic</em>", "Bold and italic")]
        [InlineData("No special characters.", "No special characters.")]
        [InlineData("<a href=\"example.com\">Link</a>", "Link")]
        [InlineData("&#60;div&#62;Encoded?&#60;/div&#62;", "&#60;div&#62;Encoded?&#60;/div&#62;")] // Not actual tags due to encoding
        public void StripHtml_HtmlCharsAndNonHtmlChars_RemovesHtmlTags(string input, string expected)
        {
            // Act
            var result = TextHelpers.StripHtml(input);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
