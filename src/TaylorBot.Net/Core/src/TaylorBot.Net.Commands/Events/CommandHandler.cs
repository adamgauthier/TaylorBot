using Discord.Commands;
using Discord.WebSocket;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Instrumentation;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Program.Events;

namespace TaylorBot.Net.Commands.Events;

public class CommandHandler(
    CommandActivityFactory commandActivityFactory,
    IServiceProvider serviceProvider,
    Lazy<ITaylorBotClient> taylorBotClient,
    CommandService commandService,
    CommandPrefixDomainService commandPrefixDomainService
    ) : IUserMessageReceivedHandler
{
    public async Task UserMessageReceivedAsync(SocketUserMessage userMessage)
    {
        if (userMessage.Author.IsBot)
            return;

        // Create a number to track where the prefix ends and the command begins
        var argPos = 0;

        var prefix = await commandPrefixDomainService.GetPrefixAsync(
            userMessage.Channel is SocketGuildChannel socketGuildChannel ? socketGuildChannel.Guild : null);

        if (!(userMessage.HasStringPrefix(prefix, ref argPos) ||
            userMessage.HasMentionPrefix(taylorBotClient.Value.DiscordShardedClient.CurrentUser, ref argPos)))
            return;

        // Execute the command with the service provider for precondition checks
        await commandService.ExecuteAsync(
            context: new TaylorBotShardedCommandContext(
                taylorBotClient.Value.DiscordShardedClient, userMessage, prefix, new(() =>
                {
                    // Create activity lazily in case the command doesn't match any module
                    var activity = commandActivityFactory.Create();
                    activity.Type = CommandType.Prefix;
                    return activity;
                })
            ),
            argPos: argPos,
            services: serviceProvider,
            multiMatchHandling: MultiMatchHandling.Best
        );
    }
}
