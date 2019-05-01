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
        private readonly ILogger<QuickStartDomainService> logger;
        private IOptionsMonitor<QuickStartEmbedOptions> optionsMonitor;
        private TaylorBotClient taylorBotClient;

        public QuickStartDomainService(ILogger<QuickStartDomainService> logger, IOptionsMonitor<QuickStartEmbedOptions> optionsMonitor, TaylorBotClient taylorBotClient)
        {
            this.logger = logger;
            this.optionsMonitor = optionsMonitor;
            this.taylorBotClient = taylorBotClient;
        }

        public async Task OnGuildJoinedAsync(SocketGuild guild)
        {
            var quickStartEmbedOptions = optionsMonitor.CurrentValue;
            await guild.DefaultChannel.SendMessageAsync(embed: new EmbedBuilder()
                .WithTitle(quickStartEmbedOptions.Title)
                .WithDescription(quickStartEmbedOptions.Description)
                .WithFields(quickStartEmbedOptions.Fields.Select(field => new EmbedFieldBuilder()
                    .WithName(field.Name)
                    .WithValue(field.Value)
                ))
                .WithColor(DiscordColor.FromHexString(quickStartEmbedOptions.Color))
                .WithThumbnailUrl(taylorBotClient.DiscordShardedClient.CurrentUser.GetAvatarUrl())
                .Build()
            );
            logger.LogInformation(LogString.From($"Sent Quick Start embed in {guild.FormatLog()}."));
        }
    }
}
