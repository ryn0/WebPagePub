using WebPagePub.ChatCommander.Helpers;

namespace WebPagePub.ChatCommander.UnitTests.HelpersTests
{
    public class DateTimeHelperTests
    {
        [Theory]
        [InlineData(10, 20)]   // Both positive
        [InlineData(-20, -10)] // Both negative
        [InlineData(-10, 20)]  // Mix of negative and positive
        public void OffSetTime_ReturnsTimeBetweenOffsets(int minOffset, int maxOffset)
        {
            var now = DateTime.UtcNow;

            var result = DateTimeHelpers.OffSetTime(now, minOffset, maxOffset);

            int expectedMinOffset = (minOffset >= 0 && maxOffset >= 0) ? minOffset : Math.Min(minOffset, maxOffset);
            int expectedMaxOffset = (minOffset >= 0 && maxOffset >= 0) ? maxOffset : Math.Max(minOffset, maxOffset);

            Assert.True(result >= now.AddMinutes(expectedMinOffset) && result <= now.AddMinutes(expectedMaxOffset));
        }
    }
}