using Discord;
using System.Globalization;

namespace TaylorBot.Net.Core.Colors;

public static class DiscordColor
{
    public static Color FromHexString(string hexString)
    {
        return new Color(uint.Parse(hexString.TrimStart('#'), NumberStyles.HexNumber));
    }
}
