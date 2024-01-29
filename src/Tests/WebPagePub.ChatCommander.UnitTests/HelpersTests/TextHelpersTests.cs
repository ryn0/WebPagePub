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

        [Theory(Skip ="this isn't finished being worked on")]
        // new link to be placed in text, add link
        [InlineData(
            "\r\n<h3>Alternative Treatments and Techniques</h3>\r\n\r\n<h4>Oral Appliances</h4>\r\n\r\n<p>For example, mandibular advancement devices (MADs) are oral appliances that keep the throat open and can be a good substitute for CPAP.</p>\r\n\r\n<h4>Positional Therapy</h4>\r\n\r\n<p>Certain sleeping positions can minimize the occurrence of apnea events. Positional therapy involves altering sleeping positions, such as avoiding back sleep, to maintain an open airway.</p>\r\n\r\n<h4>Acupuncture</h4>\r\n\r\n<p>While more research is needed, some studies suggest that acupuncture could help alleviate sleep apnea symptoms by influencing the muscles and nerves involved in breathing.</p>\r\n",
            "\"Alternative Treatments and Techniques Oral Appliances For example, mandibular advancement devices (MADs) are oral appliances that keep the throat open and can be a good substitute for CPAP.",
            "mandibular advancement devices (mads)",
            @"<a href=""https://snoringmouthpiecereview.com/articles/mandibular-advancement-device"">mandibular advancement devices (mads)</a>",
            "\r\n<h3>Alternative Treatments and Techniques</h3>\r\n\r\n<h4>Oral Appliances</h4>\r\n\r\n<p>For example, <a href=\"https://snoringmouthpiecereview.com/articles/mandibular-advancement-device\">mandibular advancement devices (MADs)</a> are oral appliances that keep the throat open and can be a good substitute for CPAP.</p>\r\n\r\n<h4>Positional Therapy</h4>\r\n\r\n<p>Certain sleeping positions can minimize the occurrence of apnea events. Positional therapy involves altering sleeping positions, such as avoiding back sleep, to maintain an open airway.</p>\r\n\r\n<h4>Acupuncture</h4>\r\n\r\n<p>While more research is needed, some studies suggest that acupuncture could help alleviate sleep apnea symptoms by influencing the muscles and nerves involved in breathing.</p>\r\n")]
        // existing link already in text, do not add double link
        [InlineData(
            "\r\n<h3>Alternative Treatments and Techniques</h3>\r\n\r\n<h4>Oral Appliances</h4>\r\n\r\n<p>For example, <a href=\"https://snoringmouthpiecereview.com/articles/mandibular-advancement-device\">mandibular advancement devices (MADs)</a> are oral appliances that keep the throat open and can be a good substitute for CPAP.</p>\r\n\r\n<h4>Positional Therapy</h4>\r\n\r\n<p>Certain sleeping positions can minimize the occurrence of apnea events. Positional therapy involves altering sleeping positions, such as avoiding back sleep, to maintain an open airway.</p>\r\n\r\n<h4>Acupuncture</h4>\r\n\r\n<p>While more research is needed, some studies suggest that acupuncture could help alleviate sleep apnea symptoms by influencing the muscles and nerves involved in breathing.</p>\r\n",
            "\"Alternative Treatments and Techniques Oral Appliances For example, mandibular advancement devices (MADs) are oral appliances that keep the throat open and can be a good substitute for CPAP.",
            "mandibular advancement devices (mads)",
            @"<a href=""https://snoringmouthpiecereview.com/articles/mandibular-advancement-device"">mandibular advancement devices (mads)</a>",
            "\r\n<h3>Alternative Treatments and Techniques</h3>\r\n\r\n<h4>Oral Appliances</h4>\r\n\r\n<p>For example, <a href=\"https://snoringmouthpiecereview.com/articles/mandibular-advancement-device\">mandibular advancement devices (MADs)</a> are oral appliances that keep the throat open and can be a good substitute for CPAP.</p>\r\n\r\n<h4>Positional Therapy</h4>\r\n\r\n<p>Certain sleeping positions can minimize the occurrence of apnea events. Positional therapy involves altering sleeping positions, such as avoiding back sleep, to maintain an open airway.</p>\r\n\r\n<h4>Acupuncture</h4>\r\n\r\n<p>While more research is needed, some studies suggest that acupuncture could help alleviate sleep apnea symptoms by influencing the muscles and nerves involved in breathing.</p>\r\n")]
        // link into text which is in header, do not add link  
        [InlineData(
            "\r\n<h3>Alternative Treatments and Techniques</h3>\r\n\r\n<h4>Oral Appliances</h4>\r\n\r\n<p>For example, mandibular advancement devices (MADs) are oral appliances that keep the throat open and can be a good substitute for CPAP.</p>\r\n\r\n<h4>Positional Therapy</h4>\r\n\r\n<p>Certain sleeping positions can minimize the occurrence of apnea events. Positional therapy involves altering sleeping positions, such as avoiding back sleep, to maintain an open airway.</p>\r\n\r\n<h4>Acupuncture</h4>\r\n\r\n<p>While more research is needed, some studies suggest that acupuncture could help alleviate sleep apnea symptoms by influencing the muscles and nerves involved in breathing.</p>\r\n",
            "\"Alternative Treatments and Techniques Oral Appliances For example, mandibular advancement devices (MADs) are oral appliances that keep the throat open and can be a good substitute for CPAP.",
            "treatments and techniques",
            @"<a href=""https://snoringmouthpiecereview.com/articles/treatments-and-techniques"">treatments and techniques</a>",
            "\r\n<h3>Alternative Treatments and Techniques</h3>\r\n\r\n<h4>Oral Appliances</h4>\r\n\r\n<p>For example, mandibular advancement devices (MADs) are oral appliances that keep the throat open and can be a good substitute for CPAP.</p>\r\n\r\n<h4>Positional Therapy</h4>\r\n\r\n<p>Certain sleeping positions can minimize the occurrence of apnea events. Positional therapy involves altering sleeping positions, such as avoiding back sleep, to maintain an open airway.</p>\r\n\r\n<h4>Acupuncture</h4>\r\n\r\n<p>While more research is needed, some studies suggest that acupuncture could help alleviate sleep apnea symptoms by influencing the muscles and nerves involved in breathing.</p>\r\n")]
        // text is maintained beacause context is not there
        [InlineData(
            "\r\n<h3>Alternative Treatments</h3>\r\n\r\n<h4>Devices</h4>\r\n\r\n<p>For example, XYZ devices are helpful.</p>\r\n",
            "Some other context that does not include XYZ devices term.",
            "XYZ devices",
            @"<a href=""https://example.com"">XYZ devices</a>",
            "\r\n<h3>Alternative Treatments</h3>\r\n\r\n<h4>Devices</h4>\r\n\r\n<p>For example, XYZ devices are helpful.</p>\r\n")]
        public void InsertLinkInHtmlPutsInLink(string originalHtml, string context, string term, string link, string expected)
        {
            Assert.Equal(expected, 
                TextHelpers.InsertLinkInHtml(
                originalHtml,
                context,
                term,
                link));
        }
    }
}
