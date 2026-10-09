using TaylorBot.Net.UserNotifier.IntegrationTests.Scenarios;
using System.Text.Json;
using System.Text.Json.Nodes;
using TaylorBot.Net.IntegrationTests.Shared.Discord;

namespace TaylorBot.Net.UserNotifier.IntegrationTests.Discord;

public sealed record ScenarioMessage(string Id, ScenarioGuild Guild, ScenarioUser User, string Content, DateTimeOffset Timestamp)
{
    public int Type { get; init; }
    public string? AttachmentUrl { get; init; }
    public string? ReplyToId { get; init; }
    public bool IsPinned { get; init; }
    public int Flags { get; init; }
    public DateTimeOffset? EditedTimestamp { get; init; }
    public string? PreviewDescription { get; init; }
    public int PreviewCount { get; init; } = 1;
    public string AttachmentId { get; init; } = "100000000000070050";
    public int ChannelType { get; init; }
}

public sealed class NotifierDriver(NotifierDiscordApi api, Func<string, object, Task> dispatch)
{
    private long _id = 100000000000080000;

    public async Task<ScenarioMessage> MessageAsync(ScenarioGuild guild, ScenarioUser user, string content, int type = 0,
        string? attachmentUrl = null, ScenarioMessage? replyTo = null, bool pinned = false, int flags = 0, int channelType = 0)
    {
        ScenarioMessage message = new($"{Interlocked.Increment(ref _id)}", guild, user, content, DateTimeOffset.UtcNow)
        {
            Type = type,
            AttachmentUrl = attachmentUrl,
            ReplyToId = replyTo?.Id,
            IsPinned = pinned,
            Flags = flags,
            ChannelType = channelType,
        };

        await dispatch("MESSAGE_CREATE", MessagePayload(message));

        return message;
    }

    public async Task<IReadOnlyList<ScenarioMessage>> MessagesAsync(ScenarioGuild guild, ScenarioUser user, int count)
    {
        List<ScenarioMessage> messages = [];
        for (var index = 0; index < count; index++)
        {
            messages.Add(await MessageAsync(guild, user, $"Message {index}"));
        }

        return messages;
    }

    public async Task DirectMessageAsync(ScenarioUser user, string content)
    {
        var channelId = NotifierDiscordApi.DmChannel(user.Id);
        await dispatch("CHANNEL_CREATE", new { id = channelId, type = 1, recipients = new[] { NotifierDiscordApi.User(user.Id, user.Username) } });

        ScenarioMessage message = new($"{Interlocked.Increment(ref _id)}", new("0", channelId, user), user, content, DateTimeOffset.UtcNow);
        var payload = MessagePayload(message);
        payload.Remove("guild_id");

        await dispatch("MESSAGE_CREATE", payload);
    }

    public Task<DiscordOutput> EditAsync(ScenarioMessage message, string content) =>
        UpdateAsync(message with { Content = content, EditedTimestamp = DateTimeOffset.UtcNow });

    public async Task<DiscordOutput> UpdateAsync(ScenarioMessage message)
    {
        var start = api.Requests.Count;
        await dispatch("MESSAGE_UPDATE", MessagePayload(message));

        return new([.. api.Requests.Skip(start)]);
    }

    public Task<DiscordOutput> RefreshEmbedsAsync(ScenarioMessage message, string description = "Link preview", int count = 1) =>
        UpdateAsync(message with { Flags = message.Flags | 1024, PreviewDescription = description, PreviewCount = count });

    public Task<DiscordOutput> SuppressEmbedsAsync(ScenarioMessage message, bool suppressed) =>
        UpdateAsync(message with { Flags = suppressed ? message.Flags | 4 : message.Flags & ~4 });

    public Task<DiscordOutput> CreateThreadAsync(ScenarioMessage message) =>
        UpdateAsync(message with { Flags = message.Flags | 32 });

    public async Task<DiscordOutput> SetPinnedAsync(ScenarioMessage message, bool pinned, bool partial = false)
    {
        var start = api.Requests.Count;
        var payload = partial
            ? JsonSerializer.SerializeToNode(new { id = message.Id, guild_id = message.Guild.Id, channel_id = message.Guild.ChannelId, pinned })!.AsObject()
            : MessagePayload(message with { IsPinned = pinned });
        await dispatch("MESSAGE_UPDATE", payload);

        return new([.. api.Requests.Skip(start)]);
    }

    public async Task<DiscordOutput> PublishAsync(ScenarioMessage message, bool partial = false)
    {
        var start = api.Requests.Count;
        var payload = partial
            ? JsonSerializer.SerializeToNode(new { id = message.Id, guild_id = message.Guild.Id, channel_id = message.Guild.ChannelId, flags = message.Flags | 1 })!.AsObject()
            : MessagePayload(message with { Flags = message.Flags | 1 });
        await dispatch("MESSAGE_UPDATE", payload);

        return new([.. api.Requests.Skip(start)]);
    }

    public async Task<DiscordOutput> DeleteAsync(ScenarioMessage message)
    {
        var start = api.Requests.Count;
        await dispatch("MESSAGE_DELETE", new { id = message.Id, guild_id = message.Guild.Id, channel_id = message.Guild.ChannelId });

        return new([.. api.Requests.Skip(start)]);
    }

    public async Task<DiscordOutput> LeaveAsync(ScenarioGuild guild, ScenarioUser user)
    {
        var start = api.Requests.Count;
        await dispatch("GUILD_MEMBER_REMOVE", new { guild_id = guild.Id, user = NotifierDiscordApi.User(user.Id, user.Username) });

        return new([.. api.Requests.Skip(start)]);
    }

    public async Task<DiscordOutput> JoinAsync(ScenarioGuild guild, ScenarioUser user)
    {
        var start = api.Requests.Count;
        await dispatch("GUILD_MEMBER_ADD", new
        {
            guild_id = guild.Id,
            user = NotifierDiscordApi.User(user.Id, user.Username),
            roles = Array.Empty<string>(),
            joined_at = DateTimeOffset.UtcNow,
            deaf = false,
            mute = false,
            flags = 0,
        });

        return new([.. api.Requests.Skip(start)]);
    }

    public Task RenameGuildAsync(ScenarioGuild guild, string name)
    {
        var payload = api.GetResource($"guilds/{guild.Id}");
        payload["name"] = name;
        api.Resource($"guilds/{guild.Id}", payload);

        return dispatch("GUILD_UPDATE", payload);
    }

    public Task RenameUserAsync(ScenarioGuild guild, ScenarioUser user, string username) => dispatch("GUILD_MEMBER_UPDATE", new
    {
        guild_id = guild.Id,
        user = NotifierDiscordApi.User(user.Id, username),
        roles = Array.Empty<string>(),
        joined_at = "2026-01-01T00:00:00Z",
    });

    public async Task<string> CreateChannelAsync(ScenarioGuild guild, string name)
    {
        var channelId = $"{Interlocked.Increment(ref _id)}";
        var channel = ScenarioData.Channel(guild with { ChannelId = channelId }, new(name));
        api.Resource($"channels/{channelId}", channel);

        await dispatch("CHANNEL_CREATE", channel);

        return channelId;
    }

    public async Task<DiscordOutput> BanAsync(ScenarioGuild guild, ScenarioUser user, bool unban = false)
    {
        var start = api.Requests.Count;
        await dispatch(unban ? "GUILD_BAN_REMOVE" : "GUILD_BAN_ADD", new { guild_id = guild.Id, user = NotifierDiscordApi.User(user.Id, user.Username) });

        return new([.. api.Requests.Skip(start)]);
    }

    public async Task<DiscordOutput> BulkDeleteAsync(ScenarioGuild guild, IReadOnlyList<ScenarioMessage> messages)
    {
        var start = api.Requests.Count;
        await dispatch("MESSAGE_DELETE_BULK", new { guild_id = guild.Id, channel_id = guild.ChannelId, ids = messages.Select(message => message.Id).ToArray() });

        return new([.. api.Requests.Skip(start)]);
    }

    public async Task<DiscordOutput> RemoveReactionAsync(ScenarioMessage message, ScenarioUser user)
    {
        var start = api.Requests.Count;
        await dispatch("MESSAGE_REACTION_REMOVE", new
        {
            guild_id = message.Guild.Id,
            channel_id = message.Guild.ChannelId,
            message_id = message.Id,
            user_id = user.Id,
            emoji = new { id = "100000000000070001", name = "wave", animated = false },
        });

        return new([.. api.Requests.Skip(start)]);
    }

    private static JsonObject MessagePayload(ScenarioMessage message)
    {
        var payload = JsonSerializer.SerializeToNode(new
        {
            id = message.Id,
            guild_id = message.Guild.Id,
            channel_id = message.Guild.ChannelId,
            channel_type = message.ChannelType,
            author = NotifierDiscordApi.User(message.User.Id, message.User.Username, bot: message.User.Id == NotifierDiscordApi.BotId),
            content = message.Content,
            timestamp = message.Timestamp,
            edited_timestamp = message.EditedTimestamp,
            type = message.Type,
            tts = false,
            pinned = message.IsPinned,
            mention_everyone = false,
            mentions = Array.Empty<object>(),
            mention_roles = Array.Empty<string>(),
            message_reference = message.ReplyToId == null ? null : new { message_id = message.ReplyToId, channel_id = message.Guild.ChannelId, guild_id = message.Guild.Id },
            attachments = message.AttachmentUrl == null ? [] : new[]
        {
            new { id = message.AttachmentId, filename = "photo.png", size = 1, url = message.AttachmentUrl, proxy_url = message.AttachmentUrl, content_type = "image/png", width = 1, height = 1 },
        },
            embeds = message.PreviewDescription == null || (message.Flags & 4) != 0
                ? []
                : Enumerable.Range(start: 0, count: message.PreviewCount)
                    .Select(index => new { type = "rich", title = $"Synthetic preview {index}", description = message.PreviewDescription, url = "https://example.invalid" }).ToArray(),
            components = Array.Empty<object>(),
            flags = message.Flags,
        })!.AsObject();

        DiscordMessageJson.RemoveNullProperties(payload);
        payload["edited_timestamp"] = JsonSerializer.SerializeToNode(message.EditedTimestamp);
        if (message.Guild.Id != "0")
        {
            payload["member"] = JsonSerializer.SerializeToNode(new
            {
                roles = Array.Empty<string>(),
                joined_at = "2026-01-01T00:00:00Z",
                nick = (string?)null,
                avatar = (string?)null,
                banner = (string?)null,
                premium_since = (string?)null,
                communication_disabled_until = (string?)null,
                pending = false,
                mute = false,
                deaf = false,
                flags = 0,
            });
        }

        return payload;
    }
}
