namespace WebPagePub.ChatCommander.Helpers
{
    public class DateTimeHelpers
    {
        public static DateTime OffSetTime(DateTime now, int minutesOffsetForArticleMin, int minutesOffsetForArticleMax)
        {
            DateTime startDate = now.AddMinutes(minutesOffsetForArticleMin);
            DateTime endDate = now.AddMinutes(minutesOffsetForArticleMax);

            var randomTest = new Random();
            TimeSpan timeSpan = endDate - startDate;
            TimeSpan newSpan = new(0, randomTest.Next(0, (int)timeSpan.TotalMinutes + 1), 0); // +1 to include the upper bound

            return startDate + newSpan;
        }
    }
}