using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;

namespace TaylorBot.Net.Commands.Events
{
    public class CommandHandler : IUserMessageReceivedHandler, IAllReadyHandler
    {
        private readonly ILogger<CommandHandler> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly Lazy<ITaylorBotClient> _taylorBotClient;
        private readonly CommandService _commandService;
        private readonly ICommandPrefixRepository _commandPrefixRepository;
        private readonly SingletonTaskRunner _commandUsageSingletonTaskRunner;
        private readonly ICommandUsageRepository _commandUsageRepository;

        public CommandHandler(
            ILogger<CommandHandler> logger,
            IServiceProvider serviceProvider,
            Lazy<ITaylorBotClient> taylorBotClient,
            CommandService commandService,
            ICommandPrefixRepository commandPrefixRepository,
            SingletonTaskRunner commandUsageSingletonTaskRunner,
            ICommandUsageRepository commandUsageRepository
        )
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _taylorBotClient = taylorBotClient;
            _commandService = commandService;
            _commandPrefixRepository = commandPrefixRepository;
            _commandUsageSingletonTaskRunner = commandUsageSingletonTaskRunner;
            _commandUsageRepository = commandUsageRepository;
        }

        public async Task UserMessageReceivedAsync(SocketUserMessage userMessage)
        {
            if (userMessage.Author.IsBot)
                return;

            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;

            var prefix = userMessage.Channel is SocketGuildChannel socketGuildChannel ?
                await _commandPrefixRepository.GetOrInsertGuildPrefixAsync(socketGuildChannel.Guild) :
                string.Empty;

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
}
