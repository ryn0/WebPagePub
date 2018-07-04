using System;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace WebPagePub.Web.Helpers
{
    public static class SiteUtilityHelper
    {
        public static string WebSafeMaker(string key)
        {
            var replaceRegex = Regex.Replace(key, @"[\W_-[#]]+", " ");

            var beforeTrim = replaceRegex.Trim().Replace("  ", " ").Replace(" ", "-").Replace("%", string.Empty).ToLowerInvariant();

            if (beforeTrim.EndsWith("#"))
            {
                beforeTrim = beforeTrim.TrimEnd('#');
            }

            if (beforeTrim.StartsWith("#"))
            {
                beforeTrim = beforeTrim.TrimStart('#');
            }

            return beforeTrim;
        }

        public static int RandomNumber(int min, int max)
        {
            var rand = new Random();
            var t = rand.Next(min, max);

            return t;
        }

        public static bool IsCaptchaValid(IFormCollection formCollection)
        {
            if (!string.IsNullOrWhiteSpace(formCollection["num1"]) &&
                !string.IsNullOrWhiteSpace(formCollection["num2"]) &&
                !string.IsNullOrWhiteSpace(formCollection["userans"]))
            {
                if (int.TryParse(formCollection["num1"], out int num1))
                {
                    if (int.TryParse(formCollection["num2"], out int num2))
                    {
                        if (int.TryParse(formCollection["userans"], out int userans))
                        {
                            if (num1 + num2 == userans)
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }
    }
}
