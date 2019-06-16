using System.Threading.Tasks;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.MinutesTracker.Domain;
using Discord.WebSocket;
using System;
using Microsoft.Extensions.Logging;
using TaylorBot.Net.Core.Logging;

namespace TaylorBot.Net.StatsTracker.Program.Events
{
    public class ReadyHandler : IShardReadyHandler
    {
        private readonly ILogger<ReadyHandler> logger;
        private readonly TaylorBotClient taylorBotClient;
        private readonly SingletonTaskRunner minuteSingletonTaskRunner;
        private readonly MinutesTrackerDomainService minutesTrackerDomainService;

        public ReadyHandler(
            ILogger<ReadyHandler> logger,
            TaylorBotClient taylorBotClient,
            SingletonTaskRunner minuteSingletonTaskRunner,
            MinutesTrackerDomainService minutesTrackerDomainService)
        {
            this.logger = logger;
            this.taylorBotClient = taylorBotClient;
            this.minuteSingletonTaskRunner = minuteSingletonTaskRunner;
            this.minutesTrackerDomainService = minutesTrackerDomainService;
        }

        public Task ShardReadyAsync(DiscordSocketClient shardClient)
        {
            minuteSingletonTaskRunner.StartTaskIfNotStarted(async () =>
            {
                try
                {
                    await minutesTrackerDomainService.StartMinutesAdderAsync();
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, LogString.From($"Unhandled exception in {nameof(minutesTrackerDomainService.StartMinutesAdderAsync)}."));
                    throw;
                }
            });

            return Task.CompletedTask;
        }
    }
}
