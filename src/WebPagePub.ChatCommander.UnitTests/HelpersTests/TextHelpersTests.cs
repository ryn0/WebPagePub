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
    }
}
