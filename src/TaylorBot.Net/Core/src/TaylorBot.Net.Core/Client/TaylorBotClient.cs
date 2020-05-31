using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Humanizer;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Events;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Core.Client
{
    public interface ITaylorBotClient
    {
        event Func<Task> AllShardsReady;

        DiscordShardedClient DiscordShardedClient { get; }
        DiscordRestClient DiscordRestClient { get; }

        Task StartAsync();
        Task StopAsync();

        SocketGuild ResolveRequiredGuild(SnowflakeId id);

        Task<IUser> ResolveRequiredUserAsync(SnowflakeId id);
    }

    public class TaylorBotClient : ITaylorBotClient
    {
        private readonly ILogger<TaylorBotClient> logger;
        private readonly ILogSeverityToLogLevelMapper logSeverityToLogLevelMapper;
        private readonly TaylorBotToken taylorBotToken;

        private int shardReadyCount = 0;

        private readonly AsyncEvent<Func<Task>> allShardsReadyEvent = new AsyncEvent<Func<Task>>();
        public event Func<Task> AllShardsReady
        {
            add { allShardsReadyEvent.Add(value); }
            remove { allShardsReadyEvent.Remove(value); }
        }

        public DiscordShardedClient DiscordShardedClient { get; }
        public DiscordRestClient DiscordRestClient { get; }

        public TaylorBotClient(ILogger<TaylorBotClient> logger, ILogSeverityToLogLevelMapper logSeverityToLogLevelMapper, TaylorBotToken taylorBotToken, DiscordShardedClient discordShardedClient, DiscordRestClient discordRestClient)
        {
            this.logger = logger;
            this.logSeverityToLogLevelMapper = logSeverityToLogLevelMapper;
            this.taylorBotToken = taylorBotToken;
            DiscordShardedClient = discordShardedClient;
            DiscordRestClient = discordRestClient;

            DiscordShardedClient.Log += LogAsync;
            DiscordShardedClient.ShardReady += ShardReadyAsync;
        }

        public async Task StartAsync()
        {
            await DiscordRestClient.LoginAsync(TokenType.Bot, taylorBotToken.Token);
            await Task.Delay(new TimeSpan(0, 0, 10));

            await DiscordShardedClient.LoginAsync(TokenType.Bot, taylorBotToken.Token);
            await DiscordShardedClient.StartAsync();
        }

        public async Task StopAsync()
        {
            await DiscordShardedClient.StopAsync();
            DiscordRestClient.Dispose();
        }

        private Task LogAsync(LogMessage log)
        {
            logger.Log(logSeverityToLogLevelMapper.MapFrom(log.Severity), LogString.From(log.ToString(prependTimestamp: false)));
            return Task.CompletedTask;
        }

        private Task ShardReadyAsync(DiscordSocketClient shardClient)
        {
            logger.LogInformation(LogString.From(
                $"Shard Number {shardClient.ShardId} is ready! Serving {"guild".ToQuantity(shardClient.Guilds.Count)} out of {DiscordShardedClient.Guilds.Count}."
            ));

            Interlocked.Increment(ref shardReadyCount);
            if (shardReadyCount >= DiscordShardedClient.Shards.Count)
            {
                logger.LogInformation(LogString.From(
                    $"All {"shard".ToQuantity(DiscordShardedClient.Shards.Count)} ready!"
                ));
                return allShardsReadyEvent.InvokeAsync();
            }

            return Task.CompletedTask;
        }

        public SocketGuild ResolveRequiredGuild(SnowflakeId id)
        {
            var guild = DiscordShardedClient.GetGuild(id.Id);
            if (guild == null)
            {
                throw new ArgumentException($"Could not resolve Guild ID {id}.");
            }

            return guild;
        }

        public async Task<IUser> ResolveRequiredUserAsync(SnowflakeId id)
        {
            var user = await ((IDiscordClient)DiscordShardedClient).GetUserAsync(id.Id);
            if (user == null)
            {
                var restUser = await DiscordRestClient.GetUserAsync(id.Id);
                if (restUser != null)
                {
                    return restUser;
                }

                throw new ArgumentException($"Could not resolve User ID {id}.");
            }

            return user;
        }
    }
}
