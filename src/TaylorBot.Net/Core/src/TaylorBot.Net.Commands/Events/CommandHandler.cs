using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;

namespace TaylorBot.Net.Commands.Events;

public class CommandHandler : IUserMessageReceivedHandler, IAllReadyHandler
{
    private readonly ILogger<CommandHandler> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly Lazy<ITaylorBotClient> _taylorBotClient;
    private readonly CommandService _commandService;
    private readonly SingletonTaskRunner _commandUsageSingletonTaskRunner;
    private readonly ICommandUsageRepository _commandUsageRepository;
    private readonly CommandPrefixDomainService _commandPrefixDomainService;

    public CommandHandler(
        ILogger<CommandHandler> logger,
        IServiceProvider serviceProvider,
        Lazy<ITaylorBotClient> taylorBotClient,
        CommandService commandService,
        SingletonTaskRunner commandUsageSingletonTaskRunner,
        ICommandUsageRepository commandUsageRepository,
        CommandPrefixDomainService commandPrefixDomainService
    )
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _taylorBotClient = taylorBotClient;
        _commandService = commandService;
        _commandUsageSingletonTaskRunner = commandUsageSingletonTaskRunner;
        _commandUsageRepository = commandUsageRepository;
        _commandPrefixDomainService = commandPrefixDomainService;
    }

    public async Task UserMessageReceivedAsync(SocketUserMessage userMessage)
    {
        if (userMessage.Author.IsBot)
            return;

        // Create a number to track where the prefix ends and the command begins
        var argPos = 0;

        var prefix = await _commandPrefixDomainService.GetPrefixAsync(
            userMessage.Channel is SocketGuildChannel socketGuildChannel ? socketGuildChannel.Guild : null);

        if (!(userMessage.HasStringPrefix(prefix, ref argPos) ||
            userMessage.HasMentionPrefix(_taylorBotClient.Value.DiscordShardedClient.CurrentUser, ref argPos)))
            return;

        // Execute the command with the service provider for precondition checks.
        await _commandService.ExecuteAsync(
            context: new TaylorBotShardedCommandContext(
                _taylorBotClient.Value.DiscordShardedClient, userMessage, prefix
            ),
            argPos: argPos,
            services: _serviceProvider,
            multiMatchHandling: MultiMatchHandling.Best
        );
    }

    public Task AllShardsReadyAsync()
    {
        _ = _commandUsageSingletonTaskRunner.StartTaskIfNotStarted(
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
                await _commandUsageRepository.PersistQueuedUsageCountIncrementsAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Unhandled exception in {nameof(_commandUsageRepository.PersistQueuedUsageCountIncrementsAsync)}.");
            }

            await Task.Delay(TimeSpan.FromMinutes(5));
        }
    }
}
