using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using Discord.Net.WebSockets;

namespace TaylorBot.Net.IntegrationTests.Shared.Discord.DiscordNet;

public sealed class InMemoryWebSocket : IWebSocketClient
{
    private Channel<(string Payload, TaskCompletionSource? Delivered)> _inbound =
        Channel.CreateUnbounded<(string, TaskCompletionSource?)>(new UnboundedChannelOptions { SingleReader = true });
    private Task? _pump;
    private CancellationToken _cancelToken;
    private readonly Dictionary<string, string> _headers = [];
    private readonly Lock _heartbeatLock = new();
    private TaskCompletionSource? _requestedHeartbeat;
    private int _pendingHeartbeats;
    private int _sequence = 1;

    public Func<JsonElement, IReadOnlyList<object>>? MembersRequested { get; set; }
    public bool HoldHeartbeatAcknowledgements { get; set; }
    public int HeartbeatIntervalMilliseconds { get; init; } = 45000;
    public int? HoldReadyForShard { get; init; }
    public Action<int>? Identified { get; init; }
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
        if (_inbound.Reader.Completion.IsCompleted)
        {
            _inbound = Channel.CreateUnbounded<(string, TaskCompletionSource?)>(new UnboundedChannelOptions { SingleReader = true });
        }
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

        return EnqueueAsync(JsonSerializer.Serialize(new { op = 10, d = new { heartbeat_interval = HeartbeatIntervalMilliseconds } }));
    }

    public async Task AcknowledgeHeartbeatAsync(CancellationToken cancellationToken)
    {
        Task? heartbeat = null;
        lock (_heartbeatLock)
        {
            if (_pendingHeartbeats > 0)
            {
                _pendingHeartbeats--;
            }
            else
            {
                _requestedHeartbeat ??= new(TaskCreationOptions.RunContinuationsAsynchronously);
                heartbeat = _requestedHeartbeat.Task;
            }
        }
        if (heartbeat != null)
        {
            await heartbeat.WaitAsync(cancellationToken);
        }
        else
        {
            await ReceiveAsync("""{"op":11,"d":null}""").WaitAsync(cancellationToken);
        }
    }

    private Task HeartbeatAsync()
    {
        lock (_heartbeatLock)
        {
            if (_requestedHeartbeat != null)
            {
                var delivered = _requestedHeartbeat;
                _requestedHeartbeat = null;
                return _inbound.Writer.WriteAsync(("""{"op":11,"d":null}""", delivered)).AsTask();
            }
            if (HoldHeartbeatAcknowledgements)
            {
                _pendingHeartbeats++;
                return Task.CompletedTask;
            }
        }
        return EnqueueAsync("""{"op":11,"d":null}""");
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
        lock (_heartbeatLock)
        {
            _pendingHeartbeats = 0;
            _requestedHeartbeat?.TrySetCanceled();
            _requestedHeartbeat = null;
        }
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
            1 => HeartbeatAsync(),
            2 => IdentifyAsync(message.RootElement.GetProperty("d")),
            3 => Task.CompletedTask,
            4 when message.RootElement.GetProperty("d").GetProperty("channel_id").ValueKind == JsonValueKind.Null => Task.CompletedTask,
            6 => EnqueueAsync(JsonSerializer.Serialize(new { op = 0, s = Interlocked.Increment(ref _sequence), t = "RESUMED", d = new { } })),
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
        Identified?.Invoke(ShardId);

        return HoldReadyForShard == ShardId ? Task.CompletedTask :
            EnqueueAsync(JsonSerializer.Serialize(new { op = 0, s = 1, t = "READY", d = ReadyPayload }));
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
