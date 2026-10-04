using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using Discord.Net.WebSockets;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Discord.DiscordNet;

internal sealed class InMemoryWebSocket : IWebSocketClient
{
    private readonly Channel<(string Payload, TaskCompletionSource? Delivered)> _inbound =
        Channel.CreateUnbounded<(string, TaskCompletionSource?)>(new UnboundedChannelOptions { SingleReader = true });
    private Task? _pump;
    private CancellationToken _cancelToken;
    private readonly Dictionary<string, string> _headers = [];
    public bool IsConnected { get; private set; }

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
            2 => EnqueueAsync("""
                {"op":0,"s":1,"t":"READY","d":{"v":10,"user":{"id":"100000000000000001","username":"IntegrationBot","discriminator":"0","avatar":null,"bot":true,"flags":0},"guilds":[],"private_channels":[],"session_id":"integration-session","resume_gateway_url":"wss://gateway.invalid","application":{"id":"100000000000000001","flags":0},"shard":[0,1]}}
                """),
            3 => Task.CompletedTask,
            var op => throw new InvalidOperationException($"Unexpected Gateway opcode {op}."),
        };
    }

    public void Dispose() => _inbound.Writer.TryComplete();
}
