using Npgsql;
using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TaylorBot.Net.UserNotifier.IntegrationTests.Scenarios;

internal sealed class CommitResponseDropper : IAsyncDisposable
{
    private readonly TcpListener _listener = new(IPAddress.Loopback, port: 0);
    private readonly CancellationTokenSource _stop = new(TimeSpan.FromSeconds(15));
    private readonly Task _completion;
    private volatile bool _dropCommit;
    private int _committed;

    public string ConnectionString { get; }
    public bool CommitObserved => Volatile.Read(ref _committed) != 0;

    public CommitResponseDropper(string connectionString)
    {
        NpgsqlConnectionStringBuilder destination = new(connectionString);
        ArgumentException.ThrowIfNullOrWhiteSpace(destination.Host);
        _listener.Start();
        ConnectionString = new NpgsqlConnectionStringBuilder(connectionString)
        {
            Host = IPAddress.Loopback.ToString(),
            Port = ((IPEndPoint)_listener.LocalEndpoint).Port,
            SslMode = SslMode.Disable,
            Pooling = false,
        }.ConnectionString;
        _completion = RunAsync(destination.Host, destination.Port);
    }

    private async Task RunAsync(string host, int port)
    {
        using var client = await _listener.AcceptTcpClientAsync(_stop.Token);
        using TcpClient upstream = new();
        await upstream.ConnectAsync(host, port, _stop.Token);
        var downstreamStream = client.GetStream();
        var upstreamStream = upstream.GetStream();

        var startup = await ReadStartupAsync(downstreamStream, _stop.Token);
        await upstreamStream.WriteAsync(startup, _stop.Token);
        var requests = ForwardRequestsAsync(downstreamStream, upstreamStream);
        var responses = ForwardResponsesAsync(upstreamStream, downstreamStream);
        try
        {
            var completed = await Task.WhenAny(requests, responses);
            await completed;
        }
        finally
        {
            await _stop.CancelAsync();
            client.Dispose();
            upstream.Dispose();
            await FinishAsync(requests);
            await FinishAsync(responses);
        }
    }

    private async Task ForwardRequestsAsync(NetworkStream client, NetworkStream upstream)
    {
        var checkpointUpdated = false;
        while (await ReadFrameAsync(client, _stop.Token) is { } frame)
        {
            if (frame[0] is (byte)'P' or (byte)'Q')
            {
                var sql = Encoding.UTF8.GetString(frame, index: 5, frame.Length - 5);
                checkpointUpdated |= sql.Contains("UPDATE tracking.queue_checkpoints", StringComparison.Ordinal);
                if (checkpointUpdated && sql.TrimEnd('\0') == "COMMIT")
                {
                    _dropCommit = true;
                }
            }
            await upstream.WriteAsync(frame, _stop.Token);
        }
    }

    private async Task ForwardResponsesAsync(NetworkStream upstream, NetworkStream client)
    {
        while (await ReadFrameAsync(upstream, _stop.Token) is { } frame)
        {
            if (_dropCommit && frame[0] == (byte)'C' && Encoding.UTF8.GetString(frame, index: 5, frame.Length - 5) == "COMMIT\0")
            {
                // PostgreSQL committed, but Npgsql never receives its CommandComplete response.
                Interlocked.Exchange(ref _committed, value: 1);
                return;
            }
            await client.WriteAsync(frame, _stop.Token);
        }
    }

    private static async Task<byte[]> ReadStartupAsync(NetworkStream stream, CancellationToken cancellationToken)
    {
        var length = new byte[4];
        await stream.ReadExactlyAsync(length, cancellationToken);
        var size = BinaryPrimitives.ReadInt32BigEndian(length);
        if (size is < 8 or > 16 * 1024 * 1024)
        {
            throw new InvalidDataException("Invalid PostgreSQL startup frame.");
        }
        var frame = new byte[size];
        length.CopyTo(frame, index: 0);
        await stream.ReadExactlyAsync(frame.AsMemory(start: 4), cancellationToken);
        return frame;
    }

    private static async Task<byte[]?> ReadFrameAsync(NetworkStream stream, CancellationToken cancellationToken)
    {
        var header = new byte[5];
        if (await stream.ReadAsync(header.AsMemory(start: 0, length: 1), cancellationToken) == 0)
        {
            return null;
        }
        await stream.ReadExactlyAsync(header.AsMemory(start: 1), cancellationToken);
        var size = BinaryPrimitives.ReadInt32BigEndian(header.AsSpan(start: 1));
        if (size is < 4 or > 16 * 1024 * 1024)
        {
            throw new InvalidDataException("Invalid PostgreSQL protocol frame.");
        }
        var frame = new byte[size + 1];
        header.CopyTo(frame, index: 0);
        await stream.ReadExactlyAsync(frame.AsMemory(start: 5), cancellationToken);
        return frame;
    }

    private async Task FinishAsync(Task pump)
    {
        try
        {
            await pump;
        }
        catch (Exception exception) when (_stop.IsCancellationRequested &&
            exception is OperationCanceledException or IOException or SocketException or ObjectDisposedException)
        {
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _stop.CancelAsync();
        _listener.Dispose();
        try
        {
            await FinishAsync(_completion);
        }
        finally
        {
            _stop.Dispose();
        }
    }
}
