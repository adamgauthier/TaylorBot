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

public class QuickStartDomainService(
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
            logger.LogInformation("Sent Quick Start embed in {QuickStartChannel}", quickStartChannel.FormatLog());
        }
        else
        {
            var owner = await taylorBotClient.Value.ResolveGuildUserAsync(guild, guild.OwnerId);
            if (owner != null)
            {
                try
                {
                    await owner.SendMessageAsync(embed: quickStartEmbed);
                    logger.LogInformation("Sent Quick Start embed to {GuildOwner}", owner.FormatLog());
                }
                catch (HttpException e) when (e.DiscordCode == DiscordErrorCode.CannotSendMessageToUser)
                {
                    logger.LogWarning("Can't send QuickStart embed to {GuildOwner} because of their DM settings", owner.FormatLog());
                }
            }
            else
            {
                logger.LogWarning("Could not find suitable channel or owner to send Quick Start embed in {Guild}", guild.FormatLog());
            }
        }
    }
}
