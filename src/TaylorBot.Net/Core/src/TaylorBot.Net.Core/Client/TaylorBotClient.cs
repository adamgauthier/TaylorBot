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

        ValueTask StartAsync();
        ValueTask StopAsync();

        SocketGuild ResolveRequiredGuild(SnowflakeId id);

        ValueTask<IUser> ResolveRequiredUserAsync(SnowflakeId id);
        ValueTask<IGuildUser?> ResolveGuildUserAsync(IGuild guild, SnowflakeId userId);
    }

    public class TaylorBotClient : ITaylorBotClient
    {
        private readonly ILogger<TaylorBotClient> _logger;
        private readonly ILogSeverityToLogLevelMapper _logSeverityToLogLevelMapper;
        private readonly TaylorBotToken _taylorBotToken;

        private int shardReadyCount = 0;

        private readonly AsyncEvent<Func<Task>> allShardsReadyEvent = new AsyncEvent<Func<Task>>();
        public event Func<Task> AllShardsReady
        {
            add { allShardsReadyEvent.Add(value); }
            remove { allShardsReadyEvent.Remove(value); }
        }

        public DiscordShardedClient DiscordShardedClient { get; }
        public DiscordRestClient DiscordRestClient { get; }

        public TaylorBotClient(
            ILogger<TaylorBotClient> logger,
            ILogSeverityToLogLevelMapper logSeverityToLogLevelMapper,
            TaylorBotToken taylorBotToken,
            DiscordShardedClient discordShardedClient,
            DiscordRestClient discordRestClient
        )
        {
            _logger = logger;
            _logSeverityToLogLevelMapper = logSeverityToLogLevelMapper;
            _taylorBotToken = taylorBotToken;
            DiscordShardedClient = discordShardedClient;
            DiscordRestClient = discordRestClient;

            DiscordShardedClient.Log += LogAsync;
            DiscordShardedClient.ShardReady += ShardReadyAsync;
        }

        public async ValueTask StartAsync()
        {
            await DiscordRestClient.LoginAsync(TokenType.Bot, _taylorBotToken.Token);
            await Task.Delay(TimeSpan.FromSeconds(10));

            await DiscordShardedClient.LoginAsync(TokenType.Bot, _taylorBotToken.Token);
            await DiscordShardedClient.StartAsync();
        }

        public async ValueTask StopAsync()
        {
            await DiscordShardedClient.StopAsync();
            DiscordRestClient.Dispose();
        }

        private Task LogAsync(LogMessage log)
        {
            _logger.Log(_logSeverityToLogLevelMapper.MapFrom(log.Severity), log.ToString(prependTimestamp: false));
            return Task.CompletedTask;
        }

        private Task ShardReadyAsync(DiscordSocketClient shardClient)
        {
            _logger.LogInformation($"Shard Number {shardClient.ShardId} is ready! Serving {"guild".ToQuantity(shardClient.Guilds.Count)} out of {DiscordShardedClient.Guilds.Count}.");

            Interlocked.Increment(ref shardReadyCount);
            if (shardReadyCount >= DiscordShardedClient.Shards.Count)
            {
                _logger.LogInformation($"All {"shard".ToQuantity(DiscordShardedClient.Shards.Count)} ready!");
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

        public async ValueTask<IUser> ResolveRequiredUserAsync(SnowflakeId id)
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

        public async ValueTask<IGuildUser?> ResolveGuildUserAsync(IGuild guild, SnowflakeId userId)
        {
            var user = await guild.GetUserAsync(userId.Id).ConfigureAwait(false);

            if (user == null)
            {
                var restUser = await DiscordRestClient.GetGuildUserAsync(guild.Id, userId.Id);
                return restUser;
            }

            return user;
        }
    }
}
