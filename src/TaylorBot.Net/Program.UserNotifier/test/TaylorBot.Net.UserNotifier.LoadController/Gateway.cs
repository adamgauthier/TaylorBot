using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.WebSockets;
using System.Text.Json;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TaylorBot.Net.IntegrationTests.Shared.Infrastructure;
using TaylorBot.Net.UserNotifier.LoadTests;

namespace TaylorBot.Net.UserNotifier.LoadController;

internal sealed partial class Gateway(LoadProfile profile, ScenarioDatabase database, ILogger logger) : IDisposable
{
    private readonly Payloads _payloads = new(profile);
    private readonly Random _random = new(profile.Seed);
    private readonly ConcurrentDictionary<int, Session> _sessions = [];
    private readonly ConcurrentDictionary<string, long> _sent = [];
    private readonly CancellationTokenSource _stop = new(TimeSpan.FromSeconds(profile.DeadlineSeconds));
    private readonly SemaphoreSlim _outputLock = new(initialCount: 1);
    private readonly StreamWriter _samples = new("/results/samples.jsonl");
    private readonly StreamWriter _phases = new("/results/phases.jsonl");
    private readonly long[] _lastMessages = new long[profile.Guilds];
    private readonly int[] _lastChannels = new int[profile.Guilds];
    private readonly long _startedAt = Stopwatch.GetTimestamp();
    private long _operation;
    private LoadStatus _status = new("startup", Finished: false, Failure: null);
    private Task? _workload;
    private int _started;
    private long _messageId = 500_000_000_000_000_000;
    private long _restRequests;
    private long _restMessages;
    private long _unexpectedRequests;
    private WorkerSample? _lastSample;

    public async Task ConnectAsync(HttpContext context)
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            context.Response.StatusCode = 400;
            return;
        }
        using var socket = await context.WebSockets.AcceptWebSocketAsync();
        using Session session = new(socket);
        await HandleAsync(session, socket, context);
    }

    private async Task HandleAsync(Session session, WebSocket socket, HttpContext context)
    {
        var initialization = Task.CompletedTask;
        try
        {
            await session.SendAsync(new { op = 10, d = new { heartbeat_interval = 41_250 } }, _stop.Token);
            var buffer = new byte[65536];
            using MemoryStream message = new();
            while (!_stop.IsCancellationRequested && socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(buffer.AsMemory(), _stop.Token);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "closed", _stop.Token);
                    return;
                }
                await message.WriteAsync(buffer.AsMemory(start: 0, result.Count), _stop.Token);
                if (message.Length > 1_048_576)
                {
                    throw new InvalidOperationException("Unexpectedly large client Gateway request.");
                }
                if (!result.EndOfMessage)
                {
                    continue;
                }
                using var payload = JsonDocument.Parse(message.GetBuffer().AsMemory(start: 0, (int)message.Length));
                var root = payload.RootElement;
                var data = root.GetProperty("d");
                switch (root.GetProperty("op").GetInt32())
                {
                    case 1:
                        await session.SendAsync(new { op = 11, d = (object?)null }, _stop.Token);
                        break;
                    case 2:
                        await initialization;
                        session.Initialized = false;
                        session.Shard = data.TryGetProperty("shard", out var shard) ? shard[0].GetInt32() : 0;
                        _sessions[session.Shard] = session;
                        initialization = InitializeAsync(session);
                        break;
                    case 6:
                        session.Shard = int.Parse(data.GetProperty("session_id").GetString()!["synthetic-".Length..]);
                        _sessions[session.Shard] = session;
                        session.ResumeFrom(data.GetProperty("seq").GetInt64());
                        await DispatchAsync(session, "RESUMED", new { });
                        session.Initialized = true;
                        session.Generation++;
                        break;
                    case 4 when data.GetProperty("channel_id").ValueKind == JsonValueKind.Null:
                        break;
                    case 8:
                        await MembersAsync(session, data);
                        break;
                    default:
                        throw new InvalidOperationException($"Unexpected Gateway opcode {root.GetProperty("op")}.");
                }
                message.SetLength(value: 0);
            }
        }
        catch (Exception exception) when (exception is WebSocketException or OperationCanceledException &&
            (_stop.IsCancellationRequested || context.RequestAborted.IsCancellationRequested))
        {
            LogConnectionStopped(exception);
        }
        catch (Exception exception)
        {
            Fail(exception);
            throw;
        }
        finally
        {
            await initialization;
        }
    }

    private async Task InitializeAsync(Session session)
    {
        try
        {
            var guilds = Enumerable.Range(0, profile.Guilds)
                .Where(index => (int)((ulong.Parse(Payloads.GuildId(index)) >> 22) % (ulong)profile.Shards) == session.Shard)
                .ToArray();
            await DispatchAsync(session, "READY", new
            {
                v = 10,
                user = Payloads.User(Payloads.BotId, bot: true),
                guilds = guilds.Select(index => new { id = Payloads.GuildId(index), unavailable = true }).ToArray(),
                session_id = $"synthetic-{session.Shard}",
                resume_gateway_url = "ws://load-controller:8080/gateway",
                application = new { id = Payloads.BotId, flags = 0 },
                private_channels = Array.Empty<object>(),
            });
            foreach (var guild in guilds)
            {
                await DispatchAsync(session, "GUILD_CREATE", _payloads.Guild(guild));
            }
            session.Initialized = true;
            session.Generation++;
        }
        catch (Exception exception)
        {
            Fail(exception);
            throw;
        }
    }

    private async Task MembersAsync(Session session, JsonElement data)
    {
        var requested = data.GetProperty("guild_id");
        var guildIds = requested.ValueKind == JsonValueKind.Array
            ? requested.EnumerateArray().Select(guild => guild.GetString()!)
            : [requested.GetString()!];
        foreach (var guildId in guildIds)
        {
            await MembersAsync(session, data, guildId);
        }
    }

    private async Task MembersAsync(Session session, JsonElement data, string guildId)
    {
        var guild = checked((int)((ulong.Parse(guildId) >> 22) - 24_000_000_000UL));
        var count = _payloads.MemberCount(guild);
        for (var start = 0; start < count; start += 1000)
        {
            await DispatchAsync(session, "GUILD_MEMBERS_CHUNK", new
            {
                guild_id = guildId,
                members = Enumerable.Range(start, Math.Min(val1: 1000, count - start)).Select(member => _payloads.Member(guild, member)).ToArray(),
                chunk_index = start / 1000,
                chunk_count = (count + 999) / 1000,
                nonce = data.TryGetProperty("nonce", out var nonce) ? nonce.GetString() : null,
            });
        }
    }

    private async Task DispatchAsync(Session session, string name, object data)
    {
        await session.DispatchAsync(name, data, _stop.Token);
        _sent.AddOrUpdate(name, addValue: 1, (_, count) => count + 1);
    }

    private Session ForGuild(int guild) => _sessions[(int)((ulong.Parse(Payloads.GuildId(guild)) >> 22) % (ulong)profile.Shards)];

    public async Task<LoadStatus> SampleAsync(WorkerSample sample)
    {
        _lastSample = sample;
        await _outputLock.WaitAsync();
        try
        {
            await _samples.WriteLineAsync(JsonSerializer.Serialize(new
            {
                Phase = _status.Phase, Worker = sample, Sent = _sent,
                RestRequests = Interlocked.Read(ref _restRequests), RestMessages = Interlocked.Read(ref _restMessages),
                UnexpectedRequests = Interlocked.Read(ref _unexpectedRequests),
            }));
            await _samples.FlushAsync();
        }
        finally
        {
            _outputLock.Release();
        }
        if (_stop.IsCancellationRequested && !_status.Finished)
        {
            Fail(new TimeoutException("Load run exceeded its deadline."));
        }
        if (sample.ReadyShards == profile.Shards && sample.Guilds == profile.Guilds &&
            _sessions.Count == profile.Shards && _sessions.Values.All(session => session.Initialized) &&
            Interlocked.CompareExchange(ref _started, value: 1, comparand: 0) == 0)
        {
            _workload = WorkloadAsync();
        }
        return _status;
    }

    private async Task WorkloadAsync()
    {
        try
        {
            await TrafficAsync("steady", profile.SteadySeconds, profile.EventsPerSecond);
            await TrafficAsync("burst", profile.BurstSeconds, checked(profile.EventsPerSecond * profile.BurstMultiplier));
            if (profile.Reconnect)
            {
                await ReconnectAsync(fresh: false);
                await TrafficAsync("after-resume", profile.SteadySeconds, profile.EventsPerSecond);
            }
            if (profile.FreshSession)
            {
                await ReconnectAsync(fresh: true);
                await TrafficAsync("after-identify", profile.SteadySeconds, profile.EventsPerSecond);
            }
            await PhaseAsync("cooldown");
            await Task.Delay(TimeSpan.FromSeconds(profile.CooldownSeconds), _stop.Token);
            await PhaseAsync("drain");
            // This is a quiet point, not a replacement for the real host's final flush.
            while (_lastSample is not { PendingTasks: 0 } || _lastSample.ReceivedMessages != _sent.GetValueOrDefault("MESSAGE_CREATE"))
            {
                await Task.Delay(TimeSpan.FromSeconds(1), _stop.Token);
            }
            await PhaseAsync("finished");
            _status = _status with { Finished = true };
        }
        catch (Exception exception)
        {
            Fail(exception);
        }
    }

    private async Task ReconnectAsync(bool fresh)
    {
        var ready = _lastSample?.ReadyEvents ?? 0;
        await PhaseAsync(fresh ? "fresh-session" : "resume");
        var previous = _sessions.Select(pair => (Shard: pair.Key, Session: pair.Value, pair.Value.Generation)).ToArray();
        foreach (var (_, session, _) in previous)
        {
            await session.SendAsync(fresh ? new { op = 9, d = (object?)false } : new { op = 7, d = (object?)null }, _stop.Token);
        }
        while (previous.Any(pair => !_sessions.TryGetValue(pair.Shard, out var current) || !current.Initialized ||
            (fresh ? ReferenceEquals(current, pair.Session) && current.Generation <= pair.Generation : ReferenceEquals(current, pair.Session))))
        {
            await Task.Delay(TimeSpan.FromMilliseconds(100), _stop.Token);
        }
        while (_lastSample is not { } sample || sample.ReadyShards != profile.Shards ||
            (fresh && (sample.ReadyEvents < ready + profile.Shards || sample.Channels != (long)profile.Guilds * profile.ChannelsPerGuild)))
        {
            await Task.Delay(TimeSpan.FromMilliseconds(100), _stop.Token);
        }
    }

    [SuppressMessage("Security", "CA5394", Justification = "Synthetic event selection must be repeatable, not cryptographically random.")]
    private async Task TrafficAsync(string phase, int seconds, int rate)
    {
        await PhaseAsync(phase);
        var target = checked((long)seconds * rate);
        var clock = Stopwatch.StartNew();
        for (long index = 0; index < target; index++)
        {
            var due = TimeSpan.FromSeconds((double)index / rate);
            if (due > clock.Elapsed)
            {
                await Task.Delay(due - clock.Elapsed, _stop.Token);
            }
            var serial = Interlocked.Increment(ref _messageId);
            var operation = _operation++;
            var guild = (int)(operation % profile.Guilds);
            var channel = 1 + (int)((operation / profile.Guilds) % (profile.ChannelsPerGuild - 1));
            var session = ForGuild(guild);
            var previous = _lastMessages[guild];
            var action = _random.Next(maxValue: 100);
            if (action == 1 && previous != 0)
            {
                await DispatchAsync(session, "MESSAGE_UPDATE", _payloads.Message(guild, _lastChannels[guild], previous, "Edited synthetic message", edited: true));
            }
            else if (action == 2 && previous != 0)
            {
                await DispatchAsync(session, "MESSAGE_DELETE", new { id = $"{previous}", channel_id = Payloads.ChannelId(guild, _lastChannels[guild]), guild_id = Payloads.GuildId(guild) });
                _lastMessages[guild] = 0;
            }
            else if (action == 3 && previous != 0)
            {
                await DispatchAsync(session, "MESSAGE_CREATE", _payloads.Message(guild, _lastChannels[guild], serial));
                await DispatchAsync(session, "MESSAGE_DELETE_BULK", new { ids = new[] { $"{previous}", $"{serial}" }, channel_id = Payloads.ChannelId(guild, _lastChannels[guild]), guild_id = Payloads.GuildId(guild) });
                _lastMessages[guild] = 0;
            }
            else if (action == 4)
            {
                var user = profile.CachedMembersPerGuild + 1;
                await DispatchAsync(session, "GUILD_MEMBER_ADD", MemberEvent(guild, user));
                await DispatchAsync(session, "GUILD_MEMBER_UPDATE", MemberEvent(guild, user));
                await DispatchAsync(session, "GUILD_MEMBER_REMOVE", new { guild_id = Payloads.GuildId(guild), user = Payloads.User(_payloads.UserId(guild, user)) });
            }
            else if (action == 5)
            {
                await DispatchAsync(session, "GUILD_ROLE_UPDATE", new { guild_id = Payloads.GuildId(guild), role = Payloads.Role(guild, role: 0, name: "@everyone") });
                await DispatchAsync(session, "CHANNEL_UPDATE", Payloads.Channel(guild, channel, "synthetic-updated"));
            }
            else
            {
                _lastMessages[guild] = serial;
                _lastChannels[guild] = channel;
                await DispatchAsync(session, "MESSAGE_CREATE", _payloads.Message(guild, channel, serial));
            }
        }
        await _outputLock.WaitAsync(_stop.Token);
        try
        {
            await _phases.WriteLineAsync(JsonSerializer.Serialize(new { At = DateTimeOffset.UtcNow, Phase = phase, TargetOperations = target, ElapsedSeconds = clock.Elapsed.TotalSeconds }));
            await _phases.FlushAsync();
        }
        finally
        {
            _outputLock.Release();
        }
    }

    private object MemberEvent(int guild, int member) => new
    {
        guild_id = Payloads.GuildId(guild),
        user = Payloads.User(_payloads.UserId(guild, member)),
        roles = Array.Empty<string>(),
        nick = "synthetic",
        joined_at = "2026-01-01T00:00:00Z",
        deaf = false,
        mute = false,
        flags = 0,
    };

    private async Task PhaseAsync(string phase)
    {
        _status = _status with { Phase = phase };
        await File.WriteAllTextAsync("/results/phase", phase, _stop.Token);
        if (File.Exists("/results/capture-phase") && (await File.ReadAllTextAsync("/results/capture-phase", _stop.Token)).Trim() == phase)
        {
            while (!File.Exists("/results/capture-complete"))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(250), _stop.Token);
            }
        }
    }

    private void Fail(Exception exception)
    {
        if (exception is OperationCanceledException &&
            Stopwatch.GetElapsedTime(_startedAt) >= TimeSpan.FromSeconds(profile.DeadlineSeconds))
        {
            exception = new TimeoutException($"Load run exceeded its deadline in phase '{_status.Phase}'.", exception);
        }
        LogWorkloadFailed(exception);
        _status = new(_status.Phase, Finished: true, Failure: exception.ToString());
    }

    public async Task RestAsync(HttpContext context)
    {
        Interlocked.Increment(ref _restRequests);
        if (profile.RestDelayMilliseconds > 0)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(profile.RestDelayMilliseconds), context.RequestAborted);
        }
        var path = context.Request.RouteValues["path"]?.ToString()?.Trim('/') ?? "";
        if (context.Request.Method == "GET")
        {
            object? response = path switch
            {
                "gateway/bot" => new { url = "ws://load-controller:8080/gateway", shards = profile.Shards, session_start_limit = new { total = 1000, remaining = 1000, reset_after = 60000, max_concurrency = profile.Shards } },
                "users/@me" => Payloads.User(Payloads.BotId, bot: true),
                "oauth2/applications/@me" => new { id = Payloads.BotId, name = "SyntheticBot", description = "", icon = (string?)null, bot_public = true, bot_require_code_grant = false, verify_key = "synthetic", owner = Payloads.User(id: "100000000000000002"), flags = 0 },
                _ => null,
            };
            var parts = path.Split('/');
            if (parts is ["channels", var channelId])
            {
                var raw = ulong.Parse(channelId) - 300_000_000_000_000_000UL;
                response = Payloads.Channel(guild: (int)(raw / 1000), channel: (int)(raw % 1000));
            }
            else if (parts is ["guilds", var guildId, "members", var userId])
            {
                var guild = checked((int)((ulong.Parse(guildId) >> 22) - 24_000_000_000UL));
                var member = userId == Payloads.BotId ? 0 : (int)((ulong.Parse(userId) - 200_000_000_000_000_000UL) % 1_000_000);
                response = _payloads.Member(guild, member);
            }
            else if (parts is ["applications", Payloads.BotId, "commands"])
            {
                response = Array.Empty<object>();
            }
            if (response != null)
            {
                await context.Response.WriteAsJsonAsync(response);
                return;
            }
        }
        else if (context.Request.Method == "POST" && path.Split('/') is ["channels", var channelId, "messages"])
        {
            Interlocked.Increment(ref _restMessages);
            using var body = await JsonDocument.ParseAsync(context.Request.Body, cancellationToken: context.RequestAborted);
            await context.Response.WriteAsJsonAsync(new
            {
                id = $"{Interlocked.Increment(ref _messageId)}",
                channel_id = channelId,
                author = Payloads.User(Payloads.BotId, bot: true),
                content = body.RootElement.TryGetProperty("content", out var content) ? content.GetString() ?? "" : "",
                timestamp = DateTimeOffset.UtcNow.ToString("O"),
                edited_timestamp = (string?)null,
                tts = false,
                mention_everyone = false,
                mentions = Array.Empty<object>(),
                mention_roles = Array.Empty<string>(),
                attachments = Array.Empty<object>(),
                embeds = body.RootElement.TryGetProperty("embeds", out var embeds) ? embeds : JsonSerializer.SerializeToElement(Array.Empty<object>()),
                pinned = false,
                type = 0,
            });
            return;
        }
        Interlocked.Increment(ref _unexpectedRequests);
        InvalidOperationException failure = new($"Unexpected synthetic REST route: {context.Request.Method} {path}");
        Fail(failure);
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { message = failure.Message, code = 0 });
    }

    public async Task<Verification> VerifyAsync()
    {
        if (_workload != null)
        {
            await _workload;
        }
        await using var connection = database.CreateConnection();
        var persisted = await connection.QuerySingleAsync<long>("SELECT COALESCE(SUM(message_count), 0)::bigint FROM guilds.text_channels;");
        var expected = _sent.GetValueOrDefault("MESSAGE_CREATE");
        return new(_status.Finished && _status.Failure == null && _unexpectedRequests == 0 && persisted == expected &&
            _lastSample is { Errors: 0 } && _lastSample.ReceivedMessages == expected,
            expected, persisted, _lastSample?.ReceivedMessages ?? 0, _restMessages, _status);
    }

    public void Dispose()
    {
        _stop.Cancel();
        _stop.Dispose();
        _samples.Dispose();
        _phases.Dispose();
        _outputLock.Dispose();
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Synthetic Gateway connection stopped")]
    private partial void LogConnectionStopped(Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "Synthetic workload failed")]
    private partial void LogWorkloadFailed(Exception exception);

    private sealed class Session(WebSocket socket) : IDisposable
    {
        private readonly SemaphoreSlim _sendLock = new(initialCount: 1);
        private long _sequence;
        public int Shard { get; set; }
        public bool Initialized { get; set; }
        public int Generation { get; set; }
        public void ResumeFrom(long sequence) => _sequence = sequence;
        public Task DispatchAsync(string name, object data, CancellationToken token) =>
            SendAsync(new { op = 0, t = name, s = Interlocked.Increment(ref _sequence), d = data }, token);
        public async Task SendAsync(object payload, CancellationToken token)
        {
            await _sendLock.WaitAsync(token);
            try
            {
                var bytes = JsonSerializer.SerializeToUtf8Bytes(payload);
                await socket.SendAsync(bytes.AsMemory(), WebSocketMessageType.Text, endOfMessage: true, token);
            }
            finally
            {
                _sendLock.Release();
            }
        }
        public void Dispose() => _sendLock.Dispose();
    }
}

internal sealed record Verification(bool Success, long ExpectedMessages, long PersistedMessages, long ReceivedMessages, long ModerationPosts, LoadStatus Status);
