using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using Discord.Net.WebSockets;

namespace TaylorBot.Net.IntegrationTests.Shared.Discord.DiscordNet;

public sealed class InMemoryWebSocket : IWebSocketClient
{
    private readonly Channel<(string Payload, TaskCompletionSource? Delivered)> _inbound =
        Channel.CreateUnbounded<(string, TaskCompletionSource?)>(new UnboundedChannelOptions { SingleReader = true });
    private Task? _pump;
    private CancellationToken _cancelToken;
    private readonly Dictionary<string, string> _headers = [];
    private int _sequence = 1;

    public Func<JsonElement, IReadOnlyList<object>>? MembersRequested { get; set; }
    public bool IsConnected { get; private set; }
    public int ShardId { get; private set; }
    public int? RequestedIntents { get; private set; }
    private int _shardCount = 1;

    public object ReadyPayload => new
    {
        v = 10,
        user = DiscordApiStub.User(DiscordApiStub.ApplicationId, "IntegrationBot", bot: true),
        guilds = Array.Empty<object>(),
        private_channels = Array.Empty<object>(),
        session_id = $"integration-session-{ShardId}",
        resume_gateway_url = "wss://gateway.invalid",
        application = new { id = DiscordApiStub.ApplicationId, flags = 0 },
        shard = new[] { ShardId, _shardCount },
    };

    public event Func<byte[], int, int, Task>? BinaryMessage;
    public event Func<string, Task>? TextMessage;
    public event Func<Exception, Task>? Closed;

    public void SetHeader(string key, string value) => _headers[key] = value;
    public void SetCancelToken(CancellationToken cancelToken) => _cancelToken = cancelToken;

    public Task ConnectAsync(string host)
    {
        IsConnected = true;
        _pump = Task.Run(async () =>
        {
            await foreach (var message in _inbound.Reader.ReadAllAsync())
            {
                try
                {
                    if (TextMessage != null)
                    {
                        await TextMessage(message.Payload);
                    }

                    message.Delivered?.SetResult();
                }
                catch (Exception exception)
                {
                    message.Delivered?.SetException(exception);
                    throw;
                }
            }
        });

        return EnqueueAsync("""{"op":10,"d":{"heartbeat_interval":45000}}""");
    }

    private Task EnqueueAsync(string payload) => _inbound.Writer.WriteAsync((payload, null)).AsTask();

    public async Task ReceiveAsync(string payload)
    {
        TaskCompletionSource delivered = new(TaskCreationOptions.RunContinuationsAsynchronously);
        await _inbound.Writer.WriteAsync((payload, delivered));
        await delivered.Task;
    }

    public Task DispatchAsync(string eventName, object data) => ReceiveAsync(JsonSerializer.Serialize(new
    {
        op = 0,
        s = Interlocked.Increment(ref _sequence),
        t = eventName,
        d = data,
    }));

    public Task ReceiveBinaryAsync(byte[] payload) =>
        BinaryMessage?.Invoke(payload, 0, payload.Length) ?? Task.CompletedTask;

    public async Task DisconnectAsync(int closeCode = 1000)
    {
        IsConnected = false;
        _inbound.Writer.TryComplete();

        if (_pump != null)
        {
            await _pump;
        }
    }

    public Task CloseAsync(Exception exception) => Closed?.Invoke(exception) ?? Task.CompletedTask;

    public Task SendAsync(byte[] data, int index, int count, bool isText)
    {
        _cancelToken.ThrowIfCancellationRequested();
        if (!isText)
        {
            throw new InvalidOperationException("Expected a JSON Gateway message.");
        }

        using var message = JsonDocument.Parse(Encoding.UTF8.GetString(data, index, count));
        return message.RootElement.GetProperty("op").GetInt32() switch
        {
            1 => EnqueueAsync("""{"op":11,"d":null}"""),
            2 => IdentifyAsync(message.RootElement.GetProperty("d")),
            3 => Task.CompletedTask,
            8 when MembersRequested != null => EnqueueMembersAsync(message.RootElement.GetProperty("d")),
            var op => throw new InvalidOperationException($"Unexpected Gateway opcode {op}."),
        };
    }

    private Task IdentifyAsync(JsonElement identify)
    {
        RequestedIntents = identify.GetProperty("intents").GetInt32();
        if (identify.TryGetProperty("shard", out var shard))
        {
            ShardId = shard[0].GetInt32();
            _shardCount = shard[1].GetInt32();
        }

        return EnqueueAsync(JsonSerializer.Serialize(new { op = 0, s = 1, t = "READY", d = ReadyPayload }));
    }

    private async Task EnqueueMembersAsync(JsonElement request)
    {
        foreach (var chunk in MembersRequested!(request))
        {
            await EnqueueAsync(JsonSerializer.Serialize(new
            {
                op = 0,
                s = Interlocked.Increment(ref _sequence),
                t = "GUILD_MEMBERS_CHUNK",
                d = chunk,
            }));
        }
    }

    public void Dispose() => _inbound.Writer.TryComplete();
}
