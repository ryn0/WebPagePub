using WebPagePub.Core.Utilities;
using Xunit;

namespace WebPagePub.Core.UnitTests.UtilitiesTests
{
    public class StringExtensionsTests
    {
        [Theory]
        [InlineData("this is MY story", "this-is-my-story")]
        [InlineData("#ghyfg8ds8#", "ghyfg8ds8")]
        [InlineData("something C#", "something-c")]
        public void CreateKeyCorrectly(string input, string expected)
        {
            var result = input.UrlKey();

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("something.jpg", "jpg")]
        [InlineData("something else.JPG", "jpg")]
        [InlineData("something else.JPEG", "jpeg")]
        public void GetFileExtensionLower(string input, string expected)
        {
            var result = input.GetFileExtensionLower();

            Assert.Equal(expected, result);
        }
    }
}
