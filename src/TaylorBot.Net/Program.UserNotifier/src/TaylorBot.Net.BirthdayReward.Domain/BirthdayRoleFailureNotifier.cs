using Discord;
using Discord.Net;
using Microsoft.Extensions.Logging;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.BirthdayReward.Domain;

public enum BirthdayRoleOperation
{
    Assign,
    Remove,
}

public partial class BirthdayRoleFailureNotifier(
    IModLogChannelLookup modLogChannelLookup,
    IBirthdayRoleAlertRepository alertRepository,
    Lazy<ITaylorBotClient> taylorBotClient,
    SlashCommandMentioner mention,
    ILogger<BirthdayRoleFailureNotifier> logger)
{
    internal static bool IsActionable(HttpException exception) =>
        exception.DiscordCode is DiscordErrorCode.MissingPermissions or DiscordErrorCode.InsufficientPermissions or DiscordErrorCode.UnknownRole;

    public async Task NotifyAsync(SnowflakeId guildId, SnowflakeId roleId, BirthdayRoleOperation operation, bool missingRole = false)
    {
        try
        {
            var channelId = await modLogChannelLookup.GetChannelIdAsync(guildId);
            if (channelId == null)
            {
                return;
            }

            var token = Guid.NewGuid().ToString("N");
            if (!await alertRepository.TryReserveAsync(guildId, operation, token))
            {
                return;
            }

            IGuild guild = taylorBotClient.Value.ResolveRequiredGuild(guildId);
            if (await guild.GetChannelAsync(channelId.Id) is not ITextChannel channel)
            {
                LogUnavailableModLog(guild.Id, channelId);
                return;
            }

            var action = operation == BirthdayRoleOperation.Assign ? "assign" : "remove";
            var description = missingRole
                ? await mention.FormatAsync($"""
                I couldn't {action} the **birthday role** because it no longer exists ⚠️
                Use {mention.Slash("birthday role")} to **re-create it** or **remove its configuration**.
                """)
                : $"""
                I couldn't {action} the birthday role {MentionUtils.MentionRole(roleId.Id)} ⚠️
                Check **Server Settings > Roles**:
                - TaylorBot needs **Manage Roles**.
                - TaylorBot's **highest role** must be **above the birthday role**.
                """;
            var embed = EmbedFactory.CreateError(description);

            await channel.SendMessageAsync(embed: embed, allowedMentions: AllowedMentions.None);
            await alertRepository.MarkDeliveredAsync(guildId, operation, token);
        }
        catch (Exception exception)
        {
            LogNotificationFailed(exception, guildId.Id, roleId, operation);
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Birthday role notification channel {ChannelId} is unavailable in guild {GuildId}.")]
    private partial void LogUnavailableModLog(ulong guildId, SnowflakeId channelId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Could not notify guild {GuildId} about birthday role {RoleId} operation {Operation}.")]
    private partial void LogNotificationFailed(Exception exception, ulong guildId, SnowflakeId roleId, BirthdayRoleOperation operation);
}
