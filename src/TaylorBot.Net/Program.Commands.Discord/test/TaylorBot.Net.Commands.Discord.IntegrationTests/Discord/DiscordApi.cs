using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Scenarios;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Discord;

public sealed class DiscordApi : IDiscordApi
{
    public const string ApplicationId = DiscordApiStub.ApplicationId;
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
    private readonly DiscordApiStub _api;
    private readonly HashSet<string> _callbacks = [];
    private readonly HashSet<string> _webhooks = [];
    private readonly Dictionary<string, JsonObject> _interactions = [];
    private readonly Dictionary<string, JsonObject> _interactionMetadata = [];
    private readonly Dictionary<string, int> _acknowledgements = [];
    private readonly Dictionary<DiscordRequest, JsonObject> _responseMessages = [];
    private readonly HashSet<(string Channel, string Message)> _messages = [];
    private readonly HashSet<string> _logChannels = [];
    private readonly Dictionary<(string Method, string Path), HttpStatusCode> _roleOperations = [];

    public DiscordApi()
    {
        _api = new(_lock, Respond);
    }

    public void ExpectModerationLog(ScenarioGuild guild)
    {
        lock (_lock) { _logChannels.Add(guild.ChannelId); }
    }

    public IReadOnlyList<DiscordRequest> ModerationLogs(ScenarioGuild guild)
    {
        lock (_lock)
        {
            return [.. _api.Requests.Where(request => request.Path == $"channels/{guild.ChannelId}/messages"
                && request.Body is { } body && MessageReference(body) == null)];
        }
    }

    public void ExpectRequest(string method, string path, object? response = null, HttpStatusCode status = HttpStatusCode.NoContent) =>
        _api.Expect(method, path, response, status);

    public IReadOnlyList<DiscordRequest> RequestsFor(string method, string path)
    {
        lock (_lock)
        {
            return [.. _api.Requests.Where(request => request.Method == method && request.Path == path.TrimStart('/').Split('?')[0])];
        }
    }

    public void RegisterUser(ScenarioUser user) =>
        _api.Resource($"users/{user.Id}", () => DiscordDriver.UserPayload(user));

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

    public void ExpectLegacyPageEdit(ScenarioGuild guild) =>
        ExpectRequest("PATCH", $"channels/{guild.ChannelId}/messages/{ResponseMessageId}",
            DiscordMessageJson.CreateMessage([], ResponseMessageId, guild.ChannelId, type: 0), HttpStatusCode.OK);

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
        get { lock (_lock) { return [.. _api.Requests.Where(request => request.Method is "PUT" or "DELETE" && request.Path.Contains("/roles/", StringComparison.Ordinal))]; } }
    }

    public void RegisterGuild(ScenarioGuild guild, IReadOnlyList<ScenarioRole> roles)
    {
        lock (_lock)
        {
            _api.Resource($"channels/{guild.ChannelId}", new { id = guild.ChannelId, guild_id = guild.Id, type = 0, name = "general", position = 0, permission_overwrites = Array.Empty<object>() });
            foreach (var member in guild.Members)
            {
                RegisterUser(member);
                _api.Resource($"guilds/{guild.Id}/members/{member.Id}", () => new
                {
                    user = DiscordDriver.UserPayload(member),
                    roles = guild.MemberRoles.GetValueOrDefault(member.Id, []),
                    joined_at = "2026-01-01T00:00:00Z",
                    deaf = false,
                    mute = false,
                });
            }

            _api.Resource($"guilds/{guild.Id}/roles", () => roles.Select(role => role.Payload()).ToArray());
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
        var message = DiscordMessageJson.CreateMessage(content, ResponseMessageId, interaction["channel_id"]!.GetValue<string>(),
            type: previous?["type"]?.GetValue<int>() ?? 20, previous, uploads);
        if (previous == null)
        {
            message["application_id"] = ApplicationId;
            message["webhook_id"] = ApplicationId;
            message["interaction_metadata"] = _interactionMetadata[token].DeepClone();
        }

        return message;
    }

    public InvalidOperationException UnexpectedRequest(string message) => _api.UnexpectedRequest(message);

    public (HttpStatusCode Status, string Body) Send(string method, string endpoint, string? json = null, IReadOnlyList<DiscordAttachment>? attachments = null) =>
        _api.Send(method, endpoint, json, attachments);

    private (HttpStatusCode Status, string Body)? Respond(DiscordRequest request)
    {
        var (method, path, body, attachments) = request;
        if (method == "GET" && path.Split('/') is ["applications", ApplicationId, "guilds", var guildId, "commands"])
        {
            return (HttpStatusCode.OK, _guildCommands.TryGetValue(guildId, out var guildCommands) ? JsonSerializer.Serialize(guildCommands.Values) : "[]");
        }

        if (_roleOperations.TryGetValue((method, path), out var roleStatus))
        {
            return (roleStatus, roleStatus == HttpStatusCode.Forbidden ? """{"code":50013,"message":"Missing Permissions"}""" : "");
        }

        if (method == "GET" && path == $"applications/{ApplicationId}/commands")
        {
            return (HttpStatusCode.OK, JsonSerializer.Serialize(_commands.Values));
        }

        if (method == "POST" && _callbacks.Contains(path))
        {
            var callback = JsonNode.Parse(body!.Value.GetRawText())!;
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
            var message = CreateInteractionMessage(path.Split('/')[2], JsonNode.Parse(body!.Value.GetRawText())!.AsObject(), update: method == "PATCH", attachments);
            _responseMessages[request] = message;
            return (HttpStatusCode.OK, message.ToJsonString());
        }

        var segments = path.Split('/');
        if (method == "POST" && segments is ["channels", var channel, "messages"] && body is { } messageBody &&
            ((MessageReference(messageBody) is { } reference && _messages.Contains((channel, reference))) ||
            (MessageReference(messageBody) == null && _logChannels.Contains(channel))))
        {
            var message = DiscordMessageJson.CreateMessage(JsonNode.Parse(messageBody.GetRawText())!.AsObject(), ResponseMessageId, channel,
                type: MessageReference(messageBody) == null ? 0 : 19, uploads: attachments);
            _responseMessages[request] = message;
            return (HttpStatusCode.OK, message.ToJsonString());
        }

        return null;
    }

    public IReadOnlyList<DiscordRequest> ForInteraction(string id, string token)
    {
        lock (_lock)
        {
            return [.. _api.Requests.Where(request =>
                request.Path.StartsWith($"interactions/{id}/{token}/", StringComparison.Ordinal) ||
                request.Path == $"webhooks/{ApplicationId}/{token}" ||
                request.Path == $"webhooks/{ApplicationId}/{token}/messages/@original")];
        }
    }

    public IReadOnlyList<DiscordRequest> ForMessage(string channel, string id)
    {
        lock (_lock)
        {
            return [.. _api.Requests.Where(request => request.Path == $"channels/{channel}/messages"
                && request.Body is { } body && MessageReference(body) == id)];
        }
    }

    private static string? MessageReference(JsonElement body) =>
        body.TryGetProperty("message_reference", out var reference) && reference.ValueKind == JsonValueKind.Object
            ? reference.GetProperty("message_id").GetString() : null;

    public void EnsureNoUnexpectedRequests() => _api.EnsureNoUnexpectedRequests();

    public void EnsureExpectationsMet() => _api.EnsureExpectationsMet();
}
