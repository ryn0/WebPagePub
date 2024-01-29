using WebPagePub.Core.Utilities;
using Xunit;

namespace WebPagePub.Core.UnitTests.UtilitiesTests
{
    public class TextUtilitiesTests
    {
        [Fact]
        public void GetWordCount_HtmlText_HasCorrectCountOfWords()
        {
            var text = @"<p>Obstructive Sleep Apnea (OSA) is a serious sleep disorder where breathing repeatedly stops and starts during sleep due to collapsed airways. These interruptions affect sleep quality and can contribute to long-term health complications.</p>

<p>In this article, we will discuss “what is obstructive sleep apnea” by outlining the symptoms, risk factors, and treatment options for OSA, equipping you with vital information to understand and address this condition.</p>

<h2>Key Takeaways</h2>

<ul>
	<li>
	<p>Obstructive Sleep Apnea (OSA) is a common disorder characterized by repeated interruptions of breathing during sleep due to partial or complete collapse of the airway, affecting approximately 1 billion adults globally and up to 90% of those with the condition remain unaware of it.</p>
	</li>
	<li>
	<p>Risk factors for OSA include anatomical issues such as micrognathia, adenoid hypertrophy, obesity, lifestyle choices like smoking and alcohol use, as well as inherited physical traits, while the severity is measured by the apnea-hypopnea index (AHI).</p>
	</li>
	<li>
	<p>Managing OSA involves several strategies such as medical treatments including Continuous Positive Airway Pressure (CPAP) and surgical interventions, as well as lifestyle changes like weight management and optimizing the sleep environment; untreated OSA can result in severe cardiovascular, cerebrovascular, and mental health complications.</p>
	</li>
</ul>";
            var result = TextUtilities.GetWordCount(text);

            Assert.Equal(191, result);
        }

        [Theory]
        [InlineData("<p>This is something.</p>", "This is something.")]
        [InlineData("This is something.", "This is something.")]
        public void StripHtml_ValidText_IsRemovedOfUnwantedCharacters(string input, string expected)
        {
            var result = TextUtilities.StripHtml(input);

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
            var result = TextUtilities.StripHtml(input);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}