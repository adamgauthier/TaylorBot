using Discord;
using Discord.WebSocket;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Colors;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Owner.Commands;

public class OwnerDiagnosticSlashCommand(Lazy<ITaylorBotClient> taylorBotClient) : ISlashCommand<NoOptions>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("owner diagnostic");

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var shardedClient = taylorBotClient.Value.DiscordShardedClient;
                BaseSocketClient socketClient = shardedClient;
                IDiscordClient client = shardedClient;

                var embed = new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .AddField("Guild Cache", (await client.GetGuildsAsync(CacheMode.CacheOnly)).Count, inline: true)
                    .AddField("DM Channels Cache", (await client.GetDMChannelsAsync(CacheMode.CacheOnly)).Count, inline: true)
                    .AddField("Shard Count", shardedClient.Shards.Count, inline: true)
                    .AddField("Latency", $"{socketClient.Latency} ms", inline: true);

                return new EmbedResult(embed.Build());
            },
            Preconditions: [
                new TaylorBotOwnerPrecondition(),
            ]
        ));
    }
}
