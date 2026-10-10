using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using TaylorBot.Net.IntegrationTests.Shared.Discord;

namespace TaylorBot.Net.UserNotifier.IntegrationTests.Discord;

public sealed class NotifierDiscordApi : IDiscordApi
{
    public const string BotId = DiscordApiStub.ApplicationId;

    private readonly Lock _lock = new();
    private readonly DiscordApiStub _api;
    private readonly Dictionary<string, object[]> _members = [];
    private long _messageId = 100000000000090000;

    public IReadOnlyList<DiscordRequest> Requests => _api.Requests;

    public NotifierDiscordApi()
    {
        _api = new(_lock, Respond, matchQueryString: false);
    }

    public static object User(string id, string username = "Alice", bool bot = false) =>
        DiscordApiStub.User(id, username, bot);

    public static string DmChannel(string userId) => $"{ulong.Parse(userId) + 50000}";

    public void Resource(string path, object payload, HttpStatusCode status = HttpStatusCode.OK) =>
        _api.Resource(path, payload, status);

    public string GlobalCommand(string name)
    {
        const string id = "100000000000080001";
        Resource($"applications/{BotId}/commands", new[]
        {
            new { id, application_id = BotId, type = 1, name, description = "Scenario command", version = id },
        });
        return id;
    }

    public void Members(string guildId, object[] members)
    {
        lock (_lock) { _members[guildId] = members; }
    }

    public JsonObject GetResource(string path)
    {
        lock (_lock)
        {
            var resource = _api.FindResource(path) ?? throw new KeyNotFoundException($"No Discord resource registered for '{path}'.");
            return JsonSerializer.SerializeToNode(resource.Payload)!.AsObject();
        }
    }

    public IReadOnlyList<object> MemberChunk(JsonElement request)
    {
        var guilds = request.GetProperty("guild_id");
        var guildIds = guilds.ValueKind == JsonValueKind.Array ? guilds.EnumerateArray().Select(guild => guild.ToString()) : [guilds.ToString()];

        lock (_lock)
        {
            return [.. guildIds.Select(guildId => (object)new
            {
                guild_id = guildId,
                members = _members[guildId],
                chunk_index = 0,
                chunk_count = 1,
                nonce = request.TryGetProperty("nonce", out var nonce) ? nonce.GetString() : null,
            })];
        }
    }

    public void Expect(string method, string path, object? response = null, HttpStatusCode status = HttpStatusCode.NoContent) =>
        _api.Expect(method, path, _ => (status, response == null ? "" : JsonSerializer.Serialize(response)));

    public void ExpectMessage(string channelId) => _api.Expect("POST", $"channels/{channelId}/messages", request =>
    {
        var content = JsonNode.Parse(request.Body!.Value.GetRawText())!.AsObject();
        if (request.Body.Value.TryGetProperty("embeds", out var embeds) &&
            (embeds.GetArrayLength() > 10 || embeds.EnumerateArray().Sum(DiscordOutput.GetEmbedTextLength) > 6000))
        {
            return (HttpStatusCode.BadRequest, JsonSerializer.Serialize(new
            {
                code = 50035,
                message = "Invalid Form Body",
                errors = new { embeds = new { _errors = new[] { new { code = "MAX_EMBED_SIZE_EXCEEDED", message = "Embed size exceeds maximum size of 6000" } } } },
            }));
        }
        var message = DiscordMessageJson.CreateMessage(content, $"{Interlocked.Increment(ref _messageId)}", channelId, type: 0, uploads: request.Attachments);

        return (HttpStatusCode.OK, message.ToJsonString());
    });

    public void RejectMessage(string channelId, int code = 50007) =>
        Expect("POST", $"channels/{channelId}/messages", new { code, message = "Cannot send messages to this user" }, HttpStatusCode.Forbidden);

    public InvalidOperationException UnexpectedRequest(string message) => _api.UnexpectedRequest(message);

    public (HttpStatusCode Status, string Body) Send(string method, string endpoint, string? json = null, IReadOnlyList<DiscordAttachment>? attachments = null) =>
        _api.Send(method, endpoint, json, attachments);

    private (HttpStatusCode Status, string Body)? Respond(DiscordRequest request)
    {
        if (request.Method == "POST" && request.Path == "users/@me/channels")
        {
            var recipient = request.Body!.Value.GetProperty("recipient_id").GetString()!;
            if (_api.FindResource($"users/{recipient}") is { Status: HttpStatusCode.OK } user)
            {
                return (HttpStatusCode.OK, JsonSerializer.Serialize(new { id = DmChannel(recipient), type = 1, recipients = new[] { user.Payload } }));
            }
        }

        return null;
    }

    public void EnsureNoUnexpectedRequests() => _api.EnsureNoUnexpectedRequests();

    public void EnsureExpectationsMet() => _api.EnsureExpectationsMet();
}

public sealed record DiscordOutput(IReadOnlyList<DiscordRequest> Requests)
{
    public JsonElement Embed => Messages.Single().Body!.Value.GetProperty("embeds")[0];
    public string? Footer => Embed.GetProperty("footer").GetProperty("text").GetString();
    public int Color => Embed.GetProperty("color").GetInt32();
    public int EmbedTextLength => GetEmbedTextLength(Embed);
    public IReadOnlyList<int> MessageEmbedTextLengths => [.. Messages.Select(message =>
        message.Body!.Value.GetProperty("embeds").EnumerateArray().Sum(GetEmbedTextLength))];
    public static int GetEmbedTextLength(JsonElement embed) => TextLength(embed, "title") + TextLength(embed, "description") +
        (embed.TryGetProperty("author", out var author) && author.ValueKind != JsonValueKind.Null ? TextLength(author, "name") : 0) +
        (embed.TryGetProperty("footer", out var footer) && footer.ValueKind != JsonValueKind.Null ? TextLength(footer, "text") : 0) +
        (embed.TryGetProperty("fields", out var fields) && fields.ValueKind != JsonValueKind.Null
            ? fields.EnumerateArray().Sum(item => TextLength(item, "name") + TextLength(item, "value")) : 0);
    public string Field(string name) => Embed.GetProperty("fields").EnumerateArray()
        .Single(field => field.GetProperty("name").GetString() == name).GetProperty("value").GetString()!;
    private static int TextLength(JsonElement element, string property) =>
        element.TryGetProperty(property, out var value) ? value.GetString()?.Length ?? 0 : 0;
    public IReadOnlyList<DiscordRequest> Messages => [.. Requests.Where(request => request.Method == "POST" && request.Path.EndsWith("/messages", StringComparison.Ordinal))];
    public string Text => string.Join('\n', Messages.SelectMany(request => request.Body!.Value.TryGetProperty("embeds", out var embeds)
        ? embeds.EnumerateArray().Select(embed => embed.TryGetProperty("description", out var description) ? description.GetString() : null)
        : []));
}
