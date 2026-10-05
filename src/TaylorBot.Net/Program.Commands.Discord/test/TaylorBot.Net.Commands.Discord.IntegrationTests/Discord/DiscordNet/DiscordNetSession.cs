using System.Text.Json;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TaylorBot.Net.Core.Program.Events;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Discord.DiscordNet;

internal sealed class DiscordNetSession(DiscordApi api)
{
    private readonly List<InMemoryWebSocket> _sockets = [];
    private readonly ReadyHandler _ready = new();
    private int _sequence = 1;

    public int RequestedIntents => _sockets.Single(socket => socket.IsConnected).RequestedIntents
        ?? throw new InvalidOperationException("Gateway identification has not completed.");

    public void Configure(IServiceCollection services)
    {
        services.AddSingleton<IShardReadyHandler>(_ready);
        services.Replace(ServiceDescriptor.Singleton(new DiscordSocketConfig
        {
            TotalShards = 1,
            RestClientProvider = _ => new InMemoryRestClient(api),
            WebSocketProvider = () =>
            {
                InMemoryWebSocket socket = new();
                _sockets.Add(socket);
                return socket;
            },
        }));
    }

    public Task WaitUntilReadyAsync(CancellationToken cancellationToken) => _ready.Completion.Task.WaitAsync(cancellationToken);

    public async Task DispatchAsync(string eventName, object data, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(new
        {
            op = 0,
            s = Interlocked.Increment(ref _sequence),
            t = eventName,
            d = data,
        });

        await _sockets.Single(socket => socket.IsConnected).ReceiveAsync(payload).WaitAsync(cancellationToken);
    }

    private sealed class ReadyHandler : IShardReadyHandler
    {
        public TaskCompletionSource Completion { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task ShardReadyAsync(DiscordSocketClient shardClient)
        {
            Completion.TrySetResult();
            return Task.CompletedTask;
        }
    }
}
