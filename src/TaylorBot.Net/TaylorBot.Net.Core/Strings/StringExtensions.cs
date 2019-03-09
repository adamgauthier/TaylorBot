using Humanizer;
using System;

namespace TaylorBot.Net.Core.Strings
{
    public static class StringExtensions
    {
        public static string Truncate(this string toTruncate, uint length, string suffix = "…")
        {
            var maxLength = (int)length;

            if (toTruncate.Length <= maxLength) return toTruncate;

            if (length <= suffix.Length) return toTruncate.Substring(0, maxLength);

            maxLength = Math.Max(0, maxLength - suffix.Length);

            return toTruncate.Substring(0, maxLength) + suffix;
        }

        public static string EscapeNewLines(this string toEscape)
        {
            return toEscape.Replace("\n", @"\n");
        }

        public static string DisplayCount(this string singularWord, int count, string surroundWith = "")
        {
            return $"{surroundWith}{count}{surroundWith} {(count != 1 ? singularWord.Pluralize() : singularWord)}";
        }

        public static string DisplayCount(this string singularWord, uint count, string surroundWith = "")
        {
            return $"{surroundWith}{count}{surroundWith} {(count != 1 ? singularWord.Pluralize() : singularWord)}";
        }

        public static string DisplayCount(this string singularWord, long count, string surroundWith = "")
        {
            return $"{surroundWith}{count}{surroundWith} {(count != 1 ? singularWord.Pluralize() : singularWord)}";
        }

        public static string DisplayCount(this string singularWord, ulong count, string surroundWith = "")
        {
            return $"{surroundWith}{count}{surroundWith} {(count != 1 ? singularWord.Pluralize() : singularWord)}";
        }
    }
}
