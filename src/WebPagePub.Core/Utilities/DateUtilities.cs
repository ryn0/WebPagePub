using System;

namespace WebPagePub.Core.Utilities
{
    public class DateUtilities
    {
        public static string FormatDate(DateTime date)
        {
            var dt = date;

            string suffix;

            switch (dt.Day)
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

            return string.Format("{0:MMMM} {1}{2}, {0:yyyy}", dt, dt.Day, suffix);
        }
    }
}
