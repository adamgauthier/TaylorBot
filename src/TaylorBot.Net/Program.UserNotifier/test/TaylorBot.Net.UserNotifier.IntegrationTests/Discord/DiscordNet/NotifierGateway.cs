using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.IntegrationTests.Shared.Discord.DiscordNet;
using System.Collections.Concurrent;
using System.Text.Json;

namespace TaylorBot.Net.UserNotifier.IntegrationTests.Discord.DiscordNet;

internal sealed class NotifierGateway(NotifierDiscordApi api)
{
    private readonly ConcurrentBag<InMemoryWebSocket> _sockets = [];
    private readonly ReadyHandler _ready = new();

    public void Configure(IServiceCollection services)
    {
        services.AddSingleton<IShardReadyHandler>(_ready);
        services.Replace(ServiceDescriptor.Singleton(provider =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            _ready.Shards = configuration.GetValue("Discord:ShardCount", defaultValue: 1);

            return new DiscordSocketConfig
            {
                TotalShards = _ready.Shards,
                MessageCacheSize = configuration.GetValue("Discord:MessageCacheSize", defaultValue: 0),
                RestClientProvider = _ => new InMemoryRestClient(api),
                WebSocketProvider = () =>
                {
                    InMemoryWebSocket socket = new() { MembersRequested = api.MemberChunk };
                    _sockets.Add(socket);
                    return socket;
                },
            };
        }));
    }

    public Task WaitUntilReadyAsync(CancellationToken cancellationToken) => _ready.Completion.Task.WaitAsync(cancellationToken);

    public Task DispatchAsync(string name, object data, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.SerializeToElement(data);
        var guildId = payload.TryGetProperty("guild_id", out var guild) ? guild.GetString() :
            name.StartsWith("GUILD_", StringComparison.Ordinal) && payload.TryGetProperty("id", out guild) ? guild.GetString() : null;
        var shard = guildId == null ? 0 : (int)((ulong.Parse(guildId) >> 22) % (ulong)_ready.Shards);

        return _sockets.Single(socket => socket.IsConnected && socket.ShardId == shard).DispatchAsync(name, data).WaitAsync(cancellationToken);
    }

    public async Task RepeatReadyAsync(CancellationToken cancellationToken)
    {
        _ready.BeginRound();
        foreach (var socket in _sockets.Where(socket => socket.IsConnected))
        {
            await socket.DispatchAsync("READY", socket.ReadyPayload).WaitAsync(cancellationToken);
        }

        await WaitUntilReadyAsync(cancellationToken);
    }

    private sealed class ReadyHandler : IShardReadyHandler
    {
        private readonly Lock _lock = new();
        private readonly HashSet<int> _ready = [];

        public int Shards { get; set; } = 1;
        public TaskCompletionSource Completion { get; private set; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public void BeginRound()
        {
            lock (_lock)
            {
                _ready.Clear();
                Completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
            }
        }

        public Task ShardReadyAsync(DiscordSocketClient shardClient)
        {
            lock (_lock)
            {
                _ready.Add(shardClient.ShardId);
                if (_ready.Count == Shards)
                {
                    Completion.TrySetResult();
                }
            }

            return Task.CompletedTask;
        }
    }
}
