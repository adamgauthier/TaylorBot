using System.Net;
using System.Text.Json;

namespace TaylorBot.Net.IntegrationTests.Shared.Discord;

public sealed class DiscordApiStub(
    Lock? sync = null,
    Func<DiscordRequest, (HttpStatusCode Status, string Body)?>? respond = null,
    bool matchQueryString = true) : IDiscordApi
{
    public const string ApplicationId = "100000000000000001";

    // Sharing the adapter's lock also protects its responder and resource factories.
    private readonly Lock _lock = sync ?? new();
    private readonly Dictionary<(string Method, string Path), Queue<Func<DiscordRequest, (HttpStatusCode Status, string Body)>>> _expected = [];
    private readonly Dictionary<string, (HttpStatusCode Status, Func<object> Payload)> _resources = [];
    private readonly List<DiscordRequest> _requests = [];
    private readonly List<string> _failures = [];

    public IReadOnlyList<DiscordRequest> Requests
    {
        get
        {
            lock (_lock)
            {
                return [.. _requests];
            }
        }
    }

    public static object User(string id, string username = "Alice", bool bot = false, string? avatar = null) =>
        new { id, username, discriminator = "0", avatar, bot, flags = 0 };

    public void Resource(string path, object payload, HttpStatusCode status = HttpStatusCode.OK) =>
        Resource(path, () => payload, status);

    public void Resource(string path, Func<object> payload, HttpStatusCode status = HttpStatusCode.OK)
    {
        lock (_lock)
        {
            _resources[path] = (status, payload);
        }
    }

    public (HttpStatusCode Status, object Payload)? FindResource(string path)
    {
        lock (_lock)
        {
            return _resources.TryGetValue(path, out var resource) ? (resource.Status, resource.Payload()) : null;
        }
    }

    public void Expect(string method, string path, object? response = null, HttpStatusCode status = HttpStatusCode.NoContent)
    {
        var body = response == null ? "" : JsonSerializer.Serialize(response);
        Expect(method, path, _ => (status, body));
    }

    public void Expect(string method, string path, Func<DiscordRequest, (HttpStatusCode Status, string Body)> response)
    {
        lock (_lock)
        {
            var key = (method, path.TrimStart('/'));
            if (!_expected.TryGetValue(key, out var queue))
            {
                queue = [];
                _expected.Add(key, queue);
            }

            queue.Enqueue(response);
        }
    }

    public (HttpStatusCode Status, string Body) Send(string method, string endpoint, string? json = null, IReadOnlyList<DiscordAttachment>? attachments = null)
    {
        var path = endpoint.TrimStart('/').Split('?')[0];
        JsonElement? body = json == null ? null : JsonSerializer.Deserialize<JsonElement>(json);

        lock (_lock)
        {
            DiscordRequest request = new(method, path, body, attachments);
            _requests.Add(request);

            var expectationPath = matchQueryString ? endpoint.TrimStart('/') : path;
            if (_expected.TryGetValue((method, expectationPath), out var expected) && expected.TryDequeue(out var response))
            {
                return response(request);
            }

            if (method == "GET")
            {
                if (FindResource(path) is { } resource)
                {
                    return (resource.Status, JsonSerializer.Serialize(resource.Payload));
                }

                if (StartupResponse(path) is { } startup)
                {
                    return (HttpStatusCode.OK, startup);
                }
            }

            return respond?.Invoke(request) ?? throw UnexpectedRequest($"Unexpected Discord request: {method} {endpoint}\n{json}");
        }
    }

    private static string? StartupResponse(string path) => path switch
    {
        "gateway/bot" => """{"url":"wss://gateway.invalid","shards":1,"session_start_limit":{"total":1000,"remaining":1000,"reset_after":1000,"max_concurrency":1}}""",
        "users/@me" => JsonSerializer.Serialize(User(ApplicationId, "IntegrationBot", bot: true)),
        "oauth2/applications/@me" => """{"id":"100000000000000001","name":"IntegrationBot","description":"","icon":null,"bot_public":true,"bot_require_code_grant":false,"verify_key":"synthetic","owner":{"id":"100000000000000002","username":"Owner","discriminator":"0","avatar":null},"flags":0}""",
        _ => null,
    };

    public InvalidOperationException UnexpectedRequest(string message)
    {
        lock (_lock)
        {
            _failures.Add(message);
        }

        return new(message);
    }

    public void EnsureNoUnexpectedRequests()
    {
        lock (_lock)
        {
            if (_failures.Count != 0)
            {
                throw new InvalidOperationException(string.Join(Environment.NewLine, _failures));
            }
        }
    }

    public void EnsureExpectationsMet()
    {
        EnsureNoUnexpectedRequests();

        lock (_lock)
        {
            var unused = _expected.Where(pair => pair.Value.Count != 0)
                .Select(pair => $"{pair.Key.Method} {pair.Key.Path} ({pair.Value.Count} remaining)").ToArray();
            if (unused.Length != 0)
            {
                throw new InvalidOperationException($"Expected Discord requests were not made:\n{string.Join('\n', unused)}");
            }
        }
    }
}
