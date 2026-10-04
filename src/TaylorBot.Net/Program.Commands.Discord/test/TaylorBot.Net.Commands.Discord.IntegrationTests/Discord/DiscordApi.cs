using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Scenarios;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Discord;

public sealed record DiscordAttachment(string Name, IReadOnlyList<byte> Bytes);
public sealed record DiscordRequest(string Method, string Path, JsonElement? Body, IReadOnlyList<DiscordAttachment>? Attachments = null);

public sealed class DiscordApi
{
    public const string ApplicationId = "100000000000000001";
    internal const string ResponseMessageId = "100000000000000020";
    internal static IReadOnlyDictionary<string, string> PageReactions { get; } = new Dictionary<string, string>
    {
        ["Previous"] = "\u25c0",
        ["Next"] = "\u25b6",
        ["Cancel"] = "\u274c",
    };
    private readonly Dictionary<string, JsonObject> _commands = LoadCommands();
    private readonly Dictionary<string, Dictionary<string, JsonObject>> _guildCommands = LoadGuildCommands();
    private readonly Lock _lock = new();
    private readonly List<DiscordRequest> _requests = [];
    private readonly List<string> _failures = [];
    private readonly HashSet<string> _callbacks = [];
    private readonly HashSet<string> _webhooks = [];
    private readonly Dictionary<string, JsonObject> _interactions = [];
    private readonly Dictionary<string, JsonObject> _interactionMetadata = [];
    private readonly Dictionary<string, int> _acknowledgements = [];
    private readonly Dictionary<DiscordRequest, JsonObject> _responseMessages = [];
    private readonly HashSet<(string Channel, string Message)> _messages = [];
    private readonly HashSet<string> _logChannels = [];
    public void ExpectModerationLog(ScenarioGuild guild)
    {
        lock (_lock) { _logChannels.Add(guild.ChannelId); }
    }
    public IReadOnlyList<DiscordRequest> ModerationLogs(ScenarioGuild guild)
    {
        lock (_lock)
        {
            return [.. _requests.Where(request => request.Path == $"channels/{guild.ChannelId}/messages"
                && request.Body is { } body && MessageReference(body) == null)];
        }
    }
    private readonly Dictionary<string, Func<object>> _resources = [];
    private readonly Dictionary<(string Method, string Path), HttpStatusCode> _roleOperations = [];
    private readonly Dictionary<(string Method, string Path), Queue<(HttpStatusCode Status, string Body)>> _expectedRequests = [];

    public void ExpectRequest(string method, string path, object? response = null, HttpStatusCode status = HttpStatusCode.NoContent)
    {
        lock (_lock)
        {
            var key = (method, path.TrimStart('/'));
            if (!_expectedRequests.TryGetValue(key, out var responses))
            {
                responses = [];
                _expectedRequests.Add(key, responses);
            }
            responses.Enqueue((status, response == null ? "" : JsonSerializer.Serialize(response)));
        }
    }

    public IReadOnlyList<DiscordRequest> RequestsFor(string method, string path)
    {
        lock (_lock)
        {
            return [.. _requests.Where(request => request.Method == method && request.Path == path.TrimStart('/').Split('?')[0])];
        }
    }

    public void RegisterUser(ScenarioUser user)
    {
        lock (_lock) { _resources[$"users/{user.Id}"] = () => DiscordDriver.UserPayload(user); }
    }

    public void ExpectMessage(string channel, string message)
    {
        lock (_lock) { _messages.Add((channel, message)); }
    }

    public void ExpectLegacyPageControls(ScenarioGuild guild)
    {
        foreach (var emoji in PageReactions.Values)
        {
            ExpectRequest("PUT", $"channels/{guild.ChannelId}/messages/{ResponseMessageId}/reactions/{Uri.EscapeDataString(emoji).ToLowerInvariant()}/@me");
        }
    }

    public void ExpectLegacyPageEdit(ScenarioGuild guild)
    {
        ExpectRequest("PATCH", $"channels/{guild.ChannelId}/messages/{ResponseMessageId}", new
        {
            id = ResponseMessageId,
            channel_id = guild.ChannelId,
            author = DiscordDriver.UserPayload(new(ApplicationId, "IntegrationBot")),
            timestamp = DateTimeOffset.UtcNow,
            type = 0,
            content = "",
            attachments = Array.Empty<object>(),
            embeds = Array.Empty<object>(),
        }, HttpStatusCode.OK);
    }

    public void ExpectRoleChange(ScenarioGuild guild, ScenarioUser user, ScenarioRole role, bool remove = false, bool forbidden = false)
    {
        lock (_lock)
        {
            _roleOperations.Add((remove ? "DELETE" : "PUT", $"guilds/{guild.Id}/members/{user.Id}/roles/{role.Id}"),
                forbidden ? HttpStatusCode.Forbidden : HttpStatusCode.NoContent);
        }
    }

    public IReadOnlyList<DiscordRequest> RoleChanges
    {
        get { lock (_lock) { return [.. _requests.Where(request => request.Method is "PUT" or "DELETE" && request.Path.Contains("/roles/", StringComparison.Ordinal))]; } }
    }

    public void RegisterGuild(ScenarioGuild guild, IReadOnlyList<ScenarioRole> roles)
    {
        lock (_lock)
        {
            _resources[$"channels/{guild.ChannelId}"] = () => new { id = guild.ChannelId, guild_id = guild.Id, type = 0, name = "general", position = 0, permission_overwrites = Array.Empty<object>() };
            foreach (var member in guild.Members)
            {
                _resources[$"users/{member.Id}"] = () => DiscordDriver.UserPayload(member);
                _resources[$"guilds/{guild.Id}/members/{member.Id}"] = () => new
                {
                    user = DiscordDriver.UserPayload(member),
                    roles = guild.MemberRoles.GetValueOrDefault(member.Id, []),
                    joined_at = "2026-01-01T00:00:00Z",
                    deaf = false,
                    mute = false,
                };
            }
            _resources[$"guilds/{guild.Id}/roles"] = () => roles.Select(role => role.Payload()).ToArray();
        }
    }

    internal string GetCommandId(string name, string? guildId = null)
    {
        if (guildId != null && _guildCommands.TryGetValue(guildId, out var guildCommands) && guildCommands.TryGetValue(name, out var guildCommand))
        {
            return guildCommand["id"]!.GetValue<string>();
        }
        return _commands.TryGetValue(name, out var command)
            ? command["id"]!.GetValue<string>()
            : throw new InvalidOperationException($"No deployed slash-command definition exists for '{name}' in guild '{guildId}'.");
    }

    private static Dictionary<string, JsonObject> LoadCommands(string? guildId = null)
    {
        var folder = Path.Combine(AppContext.BaseDirectory, "Fixtures", "Commands");
        if (guildId != null)
        {
            folder = Path.Combine(folder, "guilds", guildId);
        }
        return Directory.EnumerateFiles(folder, "*.json")
            .Order(StringComparer.Ordinal)
            .Select((path, index) =>
            {
                var command = JsonNode.Parse(File.ReadAllText(path))?.AsObject()
                    ?? throw new InvalidOperationException($"Invalid slash-command definition: {path}");
                command["id"] = $"{(guildId == null ? 100000000000010000 : 100000000000020000) + index}";
                command["application_id"] = ApplicationId;
                command["type"] = 1;
                command["version"] = "100000000000000006";
                if (guildId != null)
                {
                    command["guild_id"] = guildId;
                }
                return command;
            })
            .ToDictionary(command => command["name"]!.GetValue<string>(), StringComparer.Ordinal);
    }

    private static Dictionary<string, Dictionary<string, JsonObject>> LoadGuildCommands() =>
        Directory.EnumerateDirectories(Path.Combine(AppContext.BaseDirectory, "Fixtures", "Commands", "guilds"))
            .ToDictionary(path => Path.GetFileName(path), path => LoadCommands(Path.GetFileName(path)), StringComparer.Ordinal);

    public void ExpectInteraction(JsonObject payload, DiscordExchange? modalTrigger = null)
    {
        lock (_lock)
        {
            var id = payload["id"]!.GetValue<string>();
            var token = payload["token"]!.GetValue<string>();
            _callbacks.Add($"interactions/{id}/{token}/callback");
            _webhooks.Add($"webhooks/{ApplicationId}/{token}");
            _interactions.Add(token, payload.DeepClone().AsObject());
            JsonObject metadata = new()
            {
                ["id"] = id,
                ["type"] = payload["type"]!.DeepClone(),
                ["user"] = (payload["member"]?["user"] ?? payload["user"])!.DeepClone(),
                ["authorizing_integration_owners"] = payload["authorizing_integration_owners"]!.DeepClone(),
            };
            if (payload["type"]!.GetValue<int>() == 2)
            {
                metadata["name"] = CommandName(payload["data"]!.AsObject());
            }
            if (payload["type"]!.GetValue<int>() == 3)
            {
                metadata["interacted_message_id"] = payload["message"]!["id"]!.DeepClone();
            }
            if (modalTrigger != null)
            {
                metadata["triggering_interaction_metadata"] = _interactionMetadata[modalTrigger.Requests[0].Path.Split('/')[2]].DeepClone();
            }
            _interactionMetadata.Add(token, metadata);
        }
    }

    private static string CommandName(JsonObject data)
    {
        List<string> names = [data["name"]!.GetValue<string>()];
        while (data["options"] is JsonArray options && options.FirstOrDefault() is JsonObject subcommand &&
            subcommand["type"]!.GetValue<int>() is 1 or 2)
        {
            names.Add(subcommand["name"]!.GetValue<string>());
            data = subcommand;
        }
        return string.Join(' ', names);
    }

    internal JsonObject GetResponseMessage(DiscordRequest request)
    {
        lock (_lock)
        {
            return _responseMessages.TryGetValue(request, out var message)
                ? message.DeepClone().AsObject()
                : throw new InvalidOperationException($"No returned Discord message for {request.Method} {request.Path}.");
        }
    }

    private JsonObject CreateInteractionMessage(string token, JsonObject content, bool update, IReadOnlyList<DiscordAttachment>? uploads)
    {
        var interaction = _interactions[token];
        var previous = update && _acknowledgements[token] is 6 or 7 ? interaction["message"]?.AsObject() : null;
        var message = DiscordJson.CreateMessage(content, interaction["channel_id"]!.GetValue<string>(),
            type: previous?["type"]?.GetValue<int>() ?? 20, previous, uploads);
        if (previous == null)
        {
            message["application_id"] = ApplicationId;
            message["webhook_id"] = ApplicationId;
            message["interaction_metadata"] = _interactionMetadata[token].DeepClone();
        }
        return message;
    }

    public InvalidOperationException UnexpectedRequest(string message)
    {
        lock (_lock)
        {
            _failures.Add(message);
        }
        return new(message);
    }

    public (HttpStatusCode Status, string Body) Send(string method, string endpoint, string? json = null, IReadOnlyList<DiscordAttachment>? attachments = null)
    {
        var path = endpoint.TrimStart('/').Split('?')[0];
        JsonElement? body = json != null ? JsonSerializer.Deserialize<JsonElement>(json) : null;
        lock (_lock)
        {
            DiscordRequest request = new(method, path, body, attachments);
            _requests.Add(request);
            if (_expectedRequests.TryGetValue((method, endpoint.TrimStart('/')), out var expected) && expected.TryDequeue(out var response))
            {
                return response;
            }
            if (method == "GET" && path.Split('/') is ["applications", ApplicationId, "guilds", var guildId, "commands"])
            {
                return (HttpStatusCode.OK, _guildCommands.TryGetValue(guildId, out var guildCommands) ? JsonSerializer.Serialize(guildCommands.Values) : "[]");
            }
            if (method == "GET" && _resources.TryGetValue(path, out var resource))
            {
                return (HttpStatusCode.OK, JsonSerializer.Serialize(resource()));
            }
            if (_roleOperations.TryGetValue((method, path), out var roleStatus))
            {
                return (roleStatus, roleStatus == HttpStatusCode.Forbidden ? """{"code":50013,"message":"Missing Permissions"}""" : "");
            }
            if (method == "GET")
            {
                switch (path)
                {
                    case "gateway/bot":
                        return (HttpStatusCode.OK, """
                            {"url":"wss://gateway.invalid","shards":1,"session_start_limit":{"total":1000,"remaining":1000,"reset_after":1000,"max_concurrency":1}}
                            """);
                    case "users/@me":
                        return (HttpStatusCode.OK, """
                            {"id":"100000000000000001","username":"IntegrationBot","discriminator":"0","avatar":null,"bot":true,"flags":0}
                            """);
                    case "oauth2/applications/@me":
                        return (HttpStatusCode.OK, """
                            {"id":"100000000000000001","name":"IntegrationBot","description":"","icon":null,"bot_public":true,"bot_require_code_grant":false,"verify_key":"synthetic","owner":{"id":"100000000000000002","username":"Owner","discriminator":"0","avatar":null},"flags":0}
                            """);
                    case $"applications/{ApplicationId}/commands":
                        return (HttpStatusCode.OK, JsonSerializer.Serialize(_commands.Values));
                }
            }
            if (method == "POST" && _callbacks.Contains(path))
            {
                var callback = JsonNode.Parse(json!)!;
                _acknowledgements[path.Split('/')[2]] = callback["type"]!.GetValue<int>();
                if (callback["type"]!.GetValue<int>() is 4 or 7)
                {
                    _responseMessages[request] = CreateInteractionMessage(path.Split('/')[2], callback["data"]!.AsObject(),
                        update: callback["type"]!.GetValue<int>() == 7, attachments);
                }
                return (HttpStatusCode.NoContent, "");
            }
            if (method == "DELETE" && _webhooks.Any(webhook => path == $"{webhook}/messages/@original"))
            {
                return (HttpStatusCode.NoContent, "");
            }
            if ((method == "POST" && _webhooks.Contains(path)) ||
                (method == "PATCH" && _webhooks.Any(webhook => path == $"{webhook}/messages/@original")))
            {
                var message = CreateInteractionMessage(path.Split('/')[2], JsonNode.Parse(json!)!.AsObject(), update: method == "PATCH", attachments);
                _responseMessages[request] = message;
                return (HttpStatusCode.OK, message.ToJsonString());
            }
            var segments = path.Split('/');
            if (method == "POST" && segments is ["channels", var channel, "messages"] && body is { } messageBody &&
                ((MessageReference(messageBody) is { } reference && _messages.Contains((channel, reference))) ||
                (MessageReference(messageBody) == null && _logChannels.Contains(channel))))
            {
                var message = DiscordJson.CreateMessage(JsonNode.Parse(json!)!.AsObject(), channel,
                    type: MessageReference(messageBody) == null ? 0 : 19, uploads: attachments);
                _responseMessages[request] = message;
                return (HttpStatusCode.OK, message.ToJsonString());
            }
            var error = $"Unexpected Discord request: {method} {endpoint}";
            throw UnexpectedRequest(error);
        }
    }

    public IReadOnlyList<DiscordRequest> ForInteraction(string id, string token)
    {
        lock (_lock)
        {
            return [.. _requests.Where(request =>
                request.Path.StartsWith($"interactions/{id}/{token}/", StringComparison.Ordinal) ||
                request.Path == $"webhooks/{ApplicationId}/{token}" ||
                request.Path == $"webhooks/{ApplicationId}/{token}/messages/@original")];
        }
    }

    public IReadOnlyList<DiscordRequest> ForMessage(string channel, string id)
    {
        lock (_lock)
        {
            return [.. _requests.Where(request => request.Path == $"channels/{channel}/messages"
                && request.Body is { } body && MessageReference(body) == id)];
        }
    }

    private static string? MessageReference(JsonElement body) =>
        body.TryGetProperty("message_reference", out var reference) && reference.ValueKind == JsonValueKind.Object
            ? reference.GetProperty("message_id").GetString() : null;

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
            var unused = _expectedRequests.Where(pair => pair.Value.Count != 0)
                .Select(pair => $"{pair.Key.Method} {pair.Key.Path} ({pair.Value.Count} remaining)").ToArray();
            if (unused.Length != 0)
            {
                throw new InvalidOperationException($"Expected Discord requests were not made:\n{string.Join('\n', unused)}");
            }
        }
    }
}

internal sealed class DiscordHttpHandler(DiscordApi api, ExternalApis.ExternalApi? external = null) : HttpMessageHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var uri = request.RequestUri ?? throw new InvalidOperationException("Missing HTTP request URI.");
        if (uri.Host != "discord.com" || !uri.AbsolutePath.StartsWith("/api/v10/", StringComparison.Ordinal))
        {
            if (external != null)
            {
                return await external.SendAsync(request, cancellationToken);
            }
            throw api.UnexpectedRequest($"Unconfigured external request: {request.Method} {uri}");
        }
        string? content = null;
        List<DiscordAttachment> attachments = [];
        if (request.Content is MultipartFormDataContent multipart)
        {
            foreach (var part in multipart)
            {
                if (part.Headers.ContentDisposition?.Name?.Trim('"') == "payload_json")
                {
                    content = await part.ReadAsStringAsync(cancellationToken);
                }
                else
                {
                    attachments.Add(new(part.Headers.ContentDisposition?.FileName?.Trim('"') ?? throw new InvalidOperationException("Missing attachment filename."),
                        await part.ReadAsByteArrayAsync(cancellationToken)));
                }
            }
        }
        else if (request.Content != null)
        {
            content = await request.Content.ReadAsStringAsync(cancellationToken);
        }
        var response = api.Send(request.Method.Method, uri.PathAndQuery["/api/v10/".Length..], content, attachments);
        return new(response.Status)
        {
            Content = new StringContent(response.Body, Encoding.UTF8, "application/json"),
        };
    }
}
