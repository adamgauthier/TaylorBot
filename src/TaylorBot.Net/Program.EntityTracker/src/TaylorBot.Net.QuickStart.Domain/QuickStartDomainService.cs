using Discord;
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
            logger.LogInformation($"Sent Quick Start embed in {quickStartChannel.FormatLog()}.");
        }
        else
        {
            await guild.Owner.SendMessageAsync(embed: quickStartEmbed);
            logger.LogInformation($"Sent Quick Start embed to {guild.Owner.FormatLog()}.");
        }
    }
}
