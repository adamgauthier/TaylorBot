using Humanizer;
using System;
using System.Globalization;

namespace TaylorBot.Net.Core.Time
{
    public static class DateTimeOffsetExtensions
    {
        public static string FormatFullUserDate(this DateTimeOffset date, CultureInfo culture)
        {
            return $"{date.ToString("MMMM", culture)} {date.Day.Ordinalize(culture)}, {date.ToString(@"yyyy \a\t HH:mm:ss.fff \U\T\C", culture)} ({date.Humanize(culture: culture)})";
        }

        public static string FormatShortUserDate(this DateTimeOffset date, CultureInfo culture)
        {
            return $"{date.ToString("MMMM", culture)} {date.Day.Ordinalize(culture)}, {date.ToString(@"yyyy", culture)} ({date.Humanize(culture: culture)})";
        }
    }
}
