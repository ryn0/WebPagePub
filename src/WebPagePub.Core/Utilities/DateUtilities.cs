using System;

namespace WebPagePub.Core.Utilities
{
    public class DateUtilities
    {
        public static string FriendlyFormatDate(DateTime utcDate)
        {
            string suffix;

            switch (utcDate.Day)
            {
                case 1:
                case 21:
                case 31:
                    suffix = "st";
                    break;
                case 2:
                case 22:
                    suffix = "nd";
                    break;
                case 3:
                case 23:
                    suffix = "rd";
                    break;
                default:
                    suffix = "th";
                    break;
            }

            return string.Format("{0:MMMM} {1}{2}, {0:yyyy}", utcDate, utcDate.Day, suffix);
        }

        public static string UtcFormatDate(DateTime date)
        {
            return date.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'+'fff'Z'");
        }
    }
}