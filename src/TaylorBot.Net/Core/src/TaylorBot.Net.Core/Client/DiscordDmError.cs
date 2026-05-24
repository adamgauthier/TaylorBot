using Discord;
using Discord.Net;

namespace TaylorBot.Net.Core.Client;

public static class DiscordDmError
{
    public const int CannotSendMessagesToUserDueToDmRestrictionsDiscordCode = 50278;

    public static bool IsUndeliverable(HttpException exception)
    {
        return IsUndeliverable(exception.DiscordCode);
    }

    public static bool IsUndeliverable(DiscordErrorCode? discordErrorCode)
    {
        return discordErrorCode is DiscordErrorCode.CannotSendMessageToUser ||
            (int?)discordErrorCode == CannotSendMessagesToUserDueToDmRestrictionsDiscordCode;
    }
}
