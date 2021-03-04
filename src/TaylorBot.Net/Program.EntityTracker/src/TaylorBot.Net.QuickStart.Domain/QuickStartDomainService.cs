using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.QuickStart.Domain.Options;

namespace TaylorBot.Net.QuickStart.Domain
{
    public class QuickStartDomainService
    {
        private readonly ILogger<QuickStartDomainService> _logger;
        private readonly IOptionsMonitor<QuickStartEmbedOptions> _optionsMonitor;
        private readonly ITaylorBotClient _taylorBotClient;
        private readonly QuickStartChannelFinder _quickStartChannelFinder;

        public QuickStartDomainService(
            ILogger<QuickStartDomainService> logger,
            IOptionsMonitor<QuickStartEmbedOptions> optionsMonitor,
            ITaylorBotClient taylorBotClient,
            QuickStartChannelFinder quickStartChannelFinder
        )
        {
            _logger = logger;
            _optionsMonitor = optionsMonitor;
            _taylorBotClient = taylorBotClient;
            _quickStartChannelFinder = quickStartChannelFinder;
        }

        public async Task OnGuildJoinedAsync(SocketGuild guild)
        {
            var options = _optionsMonitor.CurrentValue;

            var quickStartEmbed = new EmbedBuilder()
                .WithTitle(options.Title)
                .WithDescription(options.Description)
                .WithFields(options.Fields.Select(field => new EmbedFieldBuilder()
                    .WithName(field.Name)
                    .WithValue(field.Value)
                ))
                .WithColor(DiscordColor.FromHexString(options.Color))
                .WithThumbnailUrl(_taylorBotClient.DiscordShardedClient.CurrentUser.GetAvatarUrl())
                .Build();

            var quickStartChannel = await _quickStartChannelFinder.FindQuickStartChannelAsync<SocketGuild, SocketTextChannel>(guild);

            if (quickStartChannel != null)
            {
                await quickStartChannel.SendMessageAsync(embed: quickStartEmbed);
                _logger.LogInformation($"Sent Quick Start embed in {quickStartChannel.FormatLog()}.");
            }
            else
            {
                await guild.Owner.SendMessageAsync(embed: quickStartEmbed);
                _logger.LogInformation($"Sent Quick Start embed to {guild.Owner.FormatLog()}.");
            }
        }
    }
}
