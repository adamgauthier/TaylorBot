using Discord;
using Humanizer;
using Microsoft.Extensions.Logging;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.Core.Strings;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Mod.Domain;

public partial class ConfigurationChangeLogger(
    IModLogChannelLookup modLogChannelLookup,
    Lazy<ITaylorBotClient> client,
    ILogger<ConfigurationChangeLogger> logger)
{
    public Task LogChannelAsync(RunContext context, string setting, SnowflakeId? before, SnowflakeId? after) =>
        LogAsync(context, setting, FormatChannel(before), FormatChannel(after));

    public Task LogToggleAsync(RunContext context, string setting, bool before, bool after) =>
        LogAsync(context, setting, before ? "Enabled" : "Disabled", after ? "Enabled" : "Disabled");

    public async Task LogAsync(RunContext context, string setting, string before, string after)
    {
        if (before == after)
        {
            return;
        }

        var guild = context.Guild;
        ArgumentNullException.ThrowIfNull(guild);
        try
        {
            var channelId = await modLogChannelLookup.GetChannelIdAsync(guild.Id);
            if (channelId != null)
            {
                await SendAsync(context, channelId, setting, before, after);
            }
        }
        catch (Exception exception)
        {
            LogLookupFailed(exception, guild.Id, setting);
        }
    }

    public async Task LogModLogAsync(RunContext context, SnowflakeId? before, SnowflakeId? after)
    {
        if (before == after)
        {
            return;
        }

        if (before != null)
        {
            await SendAsync(context, before, "🛡️ Moderation log channel", FormatChannel(before), FormatChannel(after));
        }
        if (after != null)
        {
            await SendAsync(context, after, "🛡️ Moderation log channel", FormatChannel(before), FormatChannel(after));
        }
    }

    private async Task SendAsync(RunContext context, SnowflakeId channelId, string setting, string before, string after)
    {
        var guild = context.Guild;
        ArgumentNullException.ThrowIfNull(guild);
        try
        {
            var discordGuild = guild.Fetched ?? client.Value.ResolveRequiredGuild(guild.Id);
            if (await discordGuild.GetChannelAsync(channelId) is not ITextChannel channel)
            {
                LogUnavailableChannel(guild.Id, channelId, setting);
                return;
            }

            var embed = new EmbedBuilder()
                .WithTitle("Server configuration updated")
                .WithDescription(setting)
                .WithColor(TaylorBotColors.SuccessColor)
                .AddField("Moderator", context.User.FormatTagAndMention(), inline: true)
                .AddField("Before", before.Truncate(EmbedFieldBuilder.MaxFieldValueLength), inline: true)
                .AddField("After", after.Truncate(EmbedFieldBuilder.MaxFieldValueLength), inline: true)
                .WithCurrentTimestamp()
                .Build();

            await channel.SendMessageAsync(embed: embed, allowedMentions: AllowedMentions.None);
        }
        catch (Exception exception)
        {
            LogDeliveryFailed(exception, guild.Id, channelId, setting);
        }
    }

    private static string FormatChannel(SnowflakeId? channelId) =>
        channelId != null ? MentionUtils.MentionChannel(channelId) : "Not configured";

    [LoggerMessage(Level = LogLevel.Warning, Message = "Could not find moderation log configuration in guild {GuildId} for setting {Setting}.")]
    private partial void LogLookupFailed(Exception exception, SnowflakeId guildId, string setting);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Configuration audit channel {ChannelId} is unavailable in guild {GuildId} for setting {Setting}.")]
    private partial void LogUnavailableChannel(SnowflakeId guildId, SnowflakeId channelId, string setting);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Could not send configuration audit to channel {ChannelId} in guild {GuildId} for setting {Setting}.")]
    private partial void LogDeliveryFailed(Exception exception, SnowflakeId guildId, SnowflakeId channelId, string setting);
}
