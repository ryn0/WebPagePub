namespace WebPagePub.PageManager.Console.Helpers
{
    public class DateTimeHelpers
    {
        public static DateTime GetRandomDateInRange(DateTime now, int minutesFromNowMin, int minutesFromNowMax)
        {
            DateTime startDate = now.AddMinutes(minutesFromNowMin);
            DateTime endDate = now.AddMinutes(minutesFromNowMax);

            var randomTest = new Random();
            TimeSpan timeSpan = endDate - startDate;
            TimeSpan newSpan = new(0, randomTest.Next(0, (int)timeSpan.TotalMinutes + 1), 0); // +1 to include the upper bound

            return startDate + newSpan;
        }
    }
}