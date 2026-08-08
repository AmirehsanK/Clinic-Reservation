using System;
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

        public static string ToCurrency(this int price)
        {
            return price > 0 ? price.ToString("#,0") + " USD" : "Not specified";
        }

        public static string BooleanResult(this bool boolean)
        {
            return boolean ? "Yes" : "No";
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

        public static string ToUrlDateFormat(this DateTime date)
        {
            //Date 2026-08-08 => 2026-08-08
            return date.ToString("yyyy-MM-dd");
        }
    }
}
