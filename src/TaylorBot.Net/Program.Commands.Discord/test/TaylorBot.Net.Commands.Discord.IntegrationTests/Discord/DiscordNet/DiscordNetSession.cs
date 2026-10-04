using System.Text.Json;
using System.Collections.Concurrent;
using Discord.Commands;
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
    private readonly ConcurrentDictionary<ulong, TaskCompletionSource> _messages = new();

    public void ObservePrefixCommands(IServiceProvider services)
    {
        services.GetRequiredService<CommandService>().CommandExecuted += (_, context, _) =>
        {
            if (_messages.TryRemove(context.Message.Id, out var completion))
            {
                completion.SetResult();
            }

            return Task.CompletedTask;
        };
    }

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

    public Task DrainPrefixCommandsAsync(CancellationToken cancellationToken) =>
        Task.WhenAll(_messages.Values.Select(completion => completion.Task)).WaitAsync(cancellationToken);

    public async Task DispatchAsync(string eventName, object data, CancellationToken cancellationToken)
    {
        TaskCompletionSource? message = null;
        ulong messageId = 0;
        if (eventName == "MESSAGE_CREATE")
        {
            messageId = ulong.Parse(JsonSerializer.SerializeToElement(data).GetProperty("id").GetString()!);
            message = new(TaskCreationOptions.RunContinuationsAsynchronously);
            _messages.TryAdd(messageId, message);
        }

        var payload = JsonSerializer.Serialize(new
        {
            op = 0,
            s = Interlocked.Increment(ref _sequence),
            t = eventName,
            d = data,
        });

        await _sockets.Single(socket => socket.IsConnected).ReceiveAsync(payload).WaitAsync(cancellationToken);
        if (message != null)
        {
            await message.Task.WaitAsync(cancellationToken);
        }
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
