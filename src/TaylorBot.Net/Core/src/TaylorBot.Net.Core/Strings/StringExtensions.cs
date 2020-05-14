namespace TaylorBot.Net.Core.Strings
{
    public static class StringExtensions
    {
        public static string EscapeNewLines(this string toEscape)
        {
            return toEscape.Replace("\n", @"\n");
        }
    }
}
