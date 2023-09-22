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
        [InlineData(
        "Some of the most popular types are the mandibular advancement device (MAD) and the tongue-stabilizing device (TSD).",
        "mandibular advancement device (MAD)",
        "<a href=\"https://snoringmouthpiecereview.com/articles/mandibular-advancement-device\">mandibular advancement device (MAD)</a>",
        "Some of the most popular types are the <a href=\"https://snoringmouthpiecereview.com/articles/mandibular-advancement-device\">mandibular advancement device (MAD)</a> and the tongue-stabilizing device (TSD).")]
        [InlineData(
        "The mandibular advancement device (MAD) is helpful. Many prefer the mandibular advancement device (MAD) for its simplicity.",
        "mandibular advancement device (MAD)",
        "<a href=\"https://snoringmouthpiecereview.com/articles/mandibular-advancement-device\">mandibular advancement device (MAD)</a>",
        "The <a href=\"https://snoringmouthpiecereview.com/articles/mandibular-advancement-device\">mandibular advancement device (MAD)</a> is helpful. Many prefer the <a href=\"https://snoringmouthpiecereview.com/articles/mandibular-advancement-device\">mandibular advancement device (MAD)</a> for its simplicity.")]
        [InlineData(
        "John loves playing basketball. It's his favorite sport.",
        "basketball",
        "<a href=\"https://sports.com/basketball\">basketball</a>",
        "John loves playing <a href=\"https://sports.com/basketball\">basketball</a>. It's his favorite sport.")]
        public void Test_CaseInsensitiveReplace(string input, string findText, string replaceText, string expected)
        {
            var result = TextHelpers.CaseInsensitiveReplace(input, findText, replaceText);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("Some of the most popular types are the mandibular advancement device (MAD) and the tongue-stabilizing device (TSD).", "mandibular advancement device (mad)", "mandibular advancement device (MAD)")]
        [InlineData("The QUICK brown fox.", "quick", "QUICK")]
        [InlineData("Jumped over the LAZY dog.", "lazy", "LAZY")]
        [InlineData("Jumped over the LAZY dog.", "Lazy", "LAZY")]
        public void Test_FindWithExactCasing_ReturnsCorrectCasing(string input, string searchText, string expected)
        {
            // Act
            string result = TextHelpers.FindWithExactCasing(input, searchText);

            // Assert
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

        [Theory]
        [InlineData("Hello world!", "<p>Hello world!</p>", true)]
        [InlineData("Hello", "<p>Hello world!</p>", true)]
        [InlineData("Hello world!", "<h1>Hello world!</h1>", false)]
        [InlineData("Hello world!", "<p>Hi there!</p><p>Hello world!</p>", true)]
        [InlineData("Missing text", "<p>Hello world!</p>", false)]
        public void TestIsTextSurroundedByPTag(string input, string text, bool expected)
        {
            Assert.Equal(expected, TextHelpers.IsTextSurroundedByPTag(input, text));
        }

        [Theory]
        [InlineData("Hello world!", "<li>Hello world!</li>", true)]
        [InlineData("Hello", "<li>Hello world!</li>", true)]
        [InlineData("Hello world!", "<h1>Hello world!</h1>", false)]
        [InlineData("Hello world!", "<p>Hi there!</p><ul><li>Hello world!</li></li>", true)]
        [InlineData("Missing text", "<li>Hello world!</li>", false)]
        public void TestIsTextSurroundedByLiTag(string input, string text, bool expected)
        {
            Assert.Equal(expected, TextHelpers.IsTextSurroundedByLiTag(input, text));
        }
    }
}
