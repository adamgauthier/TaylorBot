using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Program.Events;

namespace TaylorBot.Net.Commands
{
    public class CommandHandler : IUserMessageReceivedHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ITaylorBotClient _taylorBotClient;
        private readonly CommandService _commandService;
        private readonly ICommandPrefixRepository _commandPrefixRepository;

        public CommandHandler(
            IServiceProvider serviceProvider,
            ITaylorBotClient taylorBotClient,
            CommandService commandService,
            ICommandPrefixRepository commandPrefixRepository
        )
        {
            _serviceProvider = serviceProvider;
            _taylorBotClient = taylorBotClient;
            _commandService = commandService;
            _commandPrefixRepository = commandPrefixRepository;
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
                userMessage.HasMentionPrefix(_taylorBotClient.DiscordShardedClient.CurrentUser, ref argPos)))
                return;

            // Execute the command with the service provider for precondition checks.
            await _commandService.ExecuteAsync(
                context: new TaylorBotShardedCommandContext(
                    _taylorBotClient.DiscordShardedClient, userMessage, prefix
                ),
                argPos: argPos,
                services: _serviceProvider,
                multiMatchHandling: MultiMatchHandling.Best
            );
        }
    }
}
