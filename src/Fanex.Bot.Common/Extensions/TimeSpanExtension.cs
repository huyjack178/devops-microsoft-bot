namespace Fanex.Bot.Common.Extensions
{
    using System;

    public static class TimeSpanExtension
    {
        public static string ToReadableString(this TimeSpan span)
        {
            string formatted = string.Format("{0}{1}{2}{3}",
                FormatDay(span),
                FormatHour(span),
                FormatMinute(span),
                FormatSecond(span));

            if (formatted.EndsWith(", "))
            {
                formatted = formatted.Substring(0, formatted.Length - 2);
            }

            if (string.IsNullOrEmpty(formatted))
            {
                formatted = "0 second";
            }

            return formatted;
        }

        private static string FormatSecond(TimeSpan span)
        {
            var pluralChar = span.Seconds == 1 ? string.Empty : "s";

            return span.Duration().Seconds > 0 ? string.Format("{0:0} second{1}", span.Seconds, pluralChar) : string.Empty;
        }

        private static string FormatMinute(TimeSpan span)
        {
            var pluralChar = span.Minutes == 1 ? string.Empty : "s";

            return span.Duration().Minutes > 0 ? string.Format("{0:0} minute{1}, ", span.Minutes, pluralChar) : string.Empty;
        }

        private static string FormatHour(TimeSpan span)
        {
            var pluralChar = span.Hours == 1 ? string.Empty : "s";

            return span.Duration().Hours > 0 ? string.Format("{0:0} hour{1}, ", span.Hours, pluralChar) : string.Empty;
        }

        private static string FormatDay(TimeSpan span)
        {
            var pluralChar = span.Days == 1 ? String.Empty : "s";

            return span.Duration().Days > 0 ? string.Format("{0:0} day{1}, ", span.Days, pluralChar) : string.Empty;
        }
    }
}