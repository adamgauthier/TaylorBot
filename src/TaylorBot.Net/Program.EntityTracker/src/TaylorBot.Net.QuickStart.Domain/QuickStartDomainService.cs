using Discord;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.QuickStart.Domain.Options;

namespace TaylorBot.Net.QuickStart.Domain;

public partial class QuickStartDomainService(
    ILogger<QuickStartDomainService> logger,
    IOptionsMonitor<QuickStartEmbedOptions> optionsMonitor,
    Lazy<ITaylorBotClient> taylorBotClient,
    QuickStartChannelFinder quickStartChannelFinder
    )
{
    public async Task OnGuildJoinedAsync(SocketGuild guild)
    {
        var options = optionsMonitor.CurrentValue;

        var quickStartEmbed = new EmbedBuilder()
            .WithTitle(options.Title)
            .WithDescription(options.Description)
            .WithFields(options.Fields.Select(field => new EmbedFieldBuilder()
                .WithName(field.Name)
                .WithValue(field.Value)
            ))
            .WithColor(DiscordColor.FromHexString(options.Color))
            .WithThumbnailUrl(taylorBotClient.Value.DiscordShardedClient.CurrentUser.GetAvatarUrl())
            .Build();

        var quickStartChannel = await quickStartChannelFinder.FindQuickStartChannelAsync<SocketGuild, SocketTextChannel>(guild);
        if (quickStartChannel != null)
        {
            await quickStartChannel.SendMessageAsync(embed: quickStartEmbed);
            LogSentQuickStartEmbedInChannel(quickStartChannel.FormatLog());
        }
        else
        {
            var owner = await taylorBotClient.Value.ResolveGuildUserAsync(guild, guild.OwnerId);
            if (owner != null)
            {
                try
                {
                    await owner.SendMessageAsync(embed: quickStartEmbed);
                    LogSentQuickStartEmbedToOwner(owner.FormatLog());
                }
                catch (HttpException e) when (e.DiscordCode == DiscordErrorCode.CannotSendMessageToUser)
                {
                    LogCannotSendQuickStartDueToDmSettings(owner.FormatLog());
                }
            }
            else
            {
                LogCouldNotFindSuitableChannelOrOwner(guild.FormatLog());
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Sent Quick Start embed in {QuickStartChannel}")]
    private partial void LogSentQuickStartEmbedInChannel(string quickStartChannel);

    [LoggerMessage(Level = LogLevel.Information, Message = "Sent Quick Start embed to {GuildOwner}")]
    private partial void LogSentQuickStartEmbedToOwner(string guildOwner);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Can't send QuickStart embed to {GuildOwner} because of their DM settings")]
    private partial void LogCannotSendQuickStartDueToDmSettings(string guildOwner);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Could not find suitable channel or owner to send Quick Start embed in {Guild}")]
    private partial void LogCouldNotFindSuitableChannelOrOwner(string guild);
}
