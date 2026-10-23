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
    private TaskCompletionSource[] _identified = [];

    public void Configure(IServiceCollection services)
    {
        services.AddSingleton<IShardReadyHandler>(_ready);
        services.Replace(ServiceDescriptor.Singleton(provider =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            _ready.Shards = configuration.GetValue("Discord:ShardCount", defaultValue: 1);
            _identified = [.. Enumerable.Range(0, _ready.Shards).Select(_ => new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously))];

            return new DiscordSocketConfig
            {
                TotalShards = configuration.GetValue<bool>("TestGateway:HoldShardInitialization") ? null : _ready.Shards,
                MessageCacheSize = configuration.GetValue("Discord:MessageCacheSize", defaultValue: 0),
                RestClientProvider = _ => new InMemoryRestClient(api),
                WebSocketProvider = () =>
                {
                    InMemoryWebSocket socket = new()
                    {
                        MembersRequested = api.MemberChunk,
                        HoldHeartbeatAcknowledgements = configuration.GetValue<bool>("TestGateway:HoldHeartbeatAcknowledgements"),
                        HoldReadyForShard = configuration.GetValue<int?>("TestGateway:HoldReadyForShard"),
                        HeartbeatIntervalMilliseconds = configuration.GetValue("TestGateway:HeartbeatIntervalMilliseconds", defaultValue: 45000),
                        Identified = id => _identified[id].TrySetResult(),
                    };
                    _sockets.Add(socket);
                    return socket;
                },
            };
        }));
    }

    public Task WaitUntilReadyAsync(CancellationToken cancellationToken) => _ready.Completion.Task.WaitAsync(cancellationToken);
    public Task WaitForShardReadyAsync(int shardId, CancellationToken cancellationToken) => _ready.WaitForShardAsync(shardId, cancellationToken);
    public Task WaitUntilIdentifiedAsync(CancellationToken cancellationToken) =>
        Task.WhenAll(_identified.Select(identified => identified.Task)).WaitAsync(cancellationToken);

    public void HoldHeartbeats()
    {
        foreach (var socket in _sockets)
        {
            socket.HoldHeartbeatAcknowledgements = true;
        }
    }

    public async Task AcknowledgeHeartbeatAsync(CancellationToken cancellationToken)
    {
        foreach (var socket in _sockets.Where(socket => socket.IsConnected))
        {
            await socket.AcknowledgeHeartbeatAsync(cancellationToken);
        }
    }

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
        private TaskCompletionSource _changed = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public int Shards { get; set; } = 1;
        public TaskCompletionSource Completion { get; private set; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async Task WaitForShardAsync(int shardId, CancellationToken cancellationToken)
        {
            while (true)
            {
                Task changed;
                lock (_lock)
                {
                    if (_ready.Contains(shardId))
                    {
                        return;
                    }
                    changed = _changed.Task;
                }
                await changed.WaitAsync(cancellationToken);
            }
        }

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
                var changed = _changed;
                _changed = new(TaskCreationOptions.RunContinuationsAsynchronously);
                changed.TrySetResult();
                if (_ready.Count == Shards)
                {
                    Completion.TrySetResult();
                }
            }

            return Task.CompletedTask;
        }
    }
}
