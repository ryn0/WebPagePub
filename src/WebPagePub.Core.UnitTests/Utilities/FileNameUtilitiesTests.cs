using WebPagePub.Core.Utilities;
using Xunit;

namespace WebPagePub.Core.UnitTests.Utilities
{
    public class FileNameUtilitiesTests
    {
        [Theory]
        [InlineData("this is MY story", "thisisMYstory")]
        [InlineData(" this is MY story ", "thisisMYstory")]
        public void RemoveSpacesInFileNameCorrectly(string input, string expected)
        {
            var result = FileNameUtilities.RemoveSpacesInFileName(input);

            Assert.Equal(expected, result);
        }
    }
}