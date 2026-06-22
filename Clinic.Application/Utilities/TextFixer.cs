using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Clinic.Application.Utilities
{
    public static class TextFixer
    {
        public static string FixText(this string text) => text?.Trim().Replace("  ", " ");
        public static string FixEmail(string email) => email.Trim().ToLower().Replace(" ", "");

        public static string RemoveHtmlTagsExceptBreak(string text) => Regex.Replace(text, @"<(?!br[\x20/>])[^<>]+>", string.Empty);
        public static string ReplaceNewLineTextArea(string text) => text?.Replace(Environment.NewLine, "<br />");
        public static string ReplaceBrToNewLine(string text) => text?.Replace("<br />", Environment.NewLine);

        public static string FixTextForUrl(this string text)
        {
            return text.Replace(" ", "-");
        }

        public static string ConvertBrToNewLine(this string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            return text.Replace("<br/>", Environment.NewLine);
        }

        public static string ConvertNewLineToBr(this string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            return text.Replace(Environment.NewLine, "<br/>");
        }

        public static string FixedEmail(this string email)
        {
            return email.Trim().ToLower();
        }

        public static string[] SplitTags(this string tags)
        {
            return tags.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string FixTitleForUrl(this string url)
        {
            return url.Replace(" ", "-").Replace("+", "").Replace("#", "");
        }

        public static string FixUrlToTitle(this string title)
        {
            return title.Replace("-", " ");
        }

        public static string StripHTML(this string input)
        {
            return Regex.Replace(input, "<.*?>", String.Empty);
        }

        public static string LongString150(this string text, int length = 150)
        {
            if (text.Length >= length)
            {
                return text.Substring(0, length) + "...";
            }

            return text;
        }

        public static string LongString100(this string text, int length = 100)
        {
            if (text.Length >= length)
            {
                return text.Substring(0, length) + "...";
            }

            return text;
        }

        public static string LongString60(this string text, int length = 60)
        {
            if (text.Length >= length)
            {
                return text.Substring(0, length) + "...";
            }

            return text;
        }

        public static string LongString40(this string text, int length = 40)
        {
            if (text.Length >= length)
            {
                return text.Substring(0, length) + "...";
            }

            return text;
        }

        public static string LongString30(this string text, int length = 30)
        {
            if (text.Length >= length)
            {
                return text.Substring(0, length) + "...";
            }

            return text;
        }

        public static string LongString20(this string text, int length = 20)
        {
            if (text.Length >= length)
            {
                return text.Substring(0, length) + "...";
            }

            return text;
        }

        public static string ToTooman(this int price)
        {
            return price > 0 ? price.ToString("#,0 تومان") : "تعیین نشده";
        }

        public static string BooleanResult(this bool boolean)
        {
            return boolean ? "بله" : "خیر";
        }

        public static int StringToPrice(this string price)
        {
            return int.Parse(price.Replace(",", ""));
        }

        public static string BytesToMegabytesString(this Int64 bytes)
        {
            var megabytes = bytes / (1024.0 * 1024.0);
            return $"{megabytes:F2} MB";  // Formats to 2 decimal places
        }

        public static DayOfWeek ConvertToPersianDayOfWeek(this DayOfWeek miladiDayOfWeek)
        {
            return miladiDayOfWeek switch
            {
                DayOfWeek.Saturday => DayOfWeek.Saturday,
                DayOfWeek.Sunday => DayOfWeek.Sunday,
                DayOfWeek.Monday => DayOfWeek.Monday,
                DayOfWeek.Tuesday => DayOfWeek.Tuesday,
                DayOfWeek.Wednesday => DayOfWeek.Wednesday,
                DayOfWeek.Thursday => DayOfWeek.Thursday,
                DayOfWeek.Friday => DayOfWeek.Friday,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public static string ToShamsiUrlFormat(this DateTime date)
        {
            //Date 06-02-1405 => 1405-02-06
            var pc = new PersianCalendar();
            return $"{pc.GetYear(date):0000}-{pc.GetMonth(date):00}-{pc.GetDayOfMonth(date):00}";
        }

        public static DateTime ConvertShamsiStringToDateTime(this string shamsiDate)
        {
            //1405-02-29
            var parts = shamsiDate.Split('-'); // parts = [1405,02,29]

            var year = int.Parse(parts[0]);
            var month = int.Parse(parts[1]);
            var day = int.Parse(parts[2]);

            var pc = new PersianCalendar();

            return pc.ToDateTime(year,month,day,0,0,0,0);
        }
    }
}
