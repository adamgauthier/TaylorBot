using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;

namespace TaylorBot.Net.Commands.Events;

public class CommandHandler(
    ILogger<CommandHandler> logger,
    IServiceProvider serviceProvider,
    Lazy<ITaylorBotClient> taylorBotClient,
    CommandService commandService,
    SingletonTaskRunner commandUsageSingletonTaskRunner,
    ICommandUsageRepository commandUsageRepository,
    CommandPrefixDomainService commandPrefixDomainService
    ) : IUserMessageReceivedHandler, IAllReadyHandler
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

        // Execute the command with the service provider for precondition checks.
        await commandService.ExecuteAsync(
            context: new TaylorBotShardedCommandContext(
                taylorBotClient.Value.DiscordShardedClient, userMessage, prefix
            ),
            argPos: argPos,
            services: serviceProvider,
            multiMatchHandling: MultiMatchHandling.Best
        );
    }

    public Task AllShardsReadyAsync()
    {
        _ = commandUsageSingletonTaskRunner.StartTaskIfNotStarted(
            StartPersistingCommandUsageAsync,
            nameof(StartPersistingCommandUsageAsync)
        );
        return Task.CompletedTask;
    }

    private async Task StartPersistingCommandUsageAsync()
    {
        while (true)
        {
            try
            {
                await commandUsageRepository.PersistQueuedUsageCountIncrementsAsync();
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Unhandled exception in {nameof(commandUsageRepository.PersistQueuedUsageCountIncrementsAsync)}.");
            }

            await Task.Delay(TimeSpan.FromMinutes(5));
        }
    }
}
