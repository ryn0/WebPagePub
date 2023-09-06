using System;
using WebPagePub.Core.Utilities;
using Xunit;

namespace WebPagePub.Core.UnitTests.Utilities
{
    public class DateUtilitiesTests
    {
        [Theory]
        [InlineData("2017-01-01T00:00:00+0000", "January 1st, 2017")]
        [InlineData("2017-01-02T00:00:00+0000", "January 2nd, 2017")]
        [InlineData("2017-01-03T00:00:00+0000", "January 3rd, 2017")]
        [InlineData("2017-01-04T00:00:00+0000", "January 4th, 2017")]
        [InlineData("2017-01-05T00:00:00+0000", "January 5th, 2017")]
        public void FriendlyFormatDateReturnsCorrectly(string input, string expected)
        {
            var result = DateUtilities.FriendlyFormatDate(Convert.ToDateTime(input).ToUniversalTime());

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("2017-01-01T00:00:00+0000", "2017-01-01T00:00:00+000Z")]
        [InlineData("2017-01-02T00:00:00+0000", "2017-01-02T00:00:00+000Z")]
        [InlineData("2017-01-03T00:00:00+0000", "2017-01-03T00:00:00+000Z")]
        [InlineData("2017-01-04T00:00:00+0000", "2017-01-04T00:00:00+000Z")]
        public void UtcFormatDateReturnsCorrectly(string input, string expected)
        {
            var result = DateUtilities.UtcFormatDate(Convert.ToDateTime(input));

            Assert.Equal(expected, result);
        }
    }
}
