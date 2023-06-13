﻿using Humanizer;
using System;
using System.Globalization;

namespace TaylorBot.Net.Core.Time;

public static class DateTimeOffsetExtensions
{
    public static string FormatFullUserDate(this DateTimeOffset date, CultureInfo culture)
    {
        var inUTC = date.ToUniversalTime();
        return $"{inUTC.ToString("MMMM", culture)} {inUTC.Day.Ordinalize(culture)}, {inUTC.ToString(@"yyyy \a\t HH:mm:ss.fff \U\T\C", culture)} ({inUTC.Humanize(culture: culture)})";
    }

    public static string FormatShortUserDate(this DateTimeOffset date, CultureInfo culture)
    {
        return $"{date.ToString("MMMM", culture)} {date.Day.Ordinalize(culture)}, {date.ToString(@"yyyy", culture)} ({date.Humanize(culture: culture)})";
    }

    public static string FormatShortUserLogDate(this DateTimeOffset date)
    {
        return $"<t:{date.ToUnixTimeSeconds()}:R>";
    }
}
