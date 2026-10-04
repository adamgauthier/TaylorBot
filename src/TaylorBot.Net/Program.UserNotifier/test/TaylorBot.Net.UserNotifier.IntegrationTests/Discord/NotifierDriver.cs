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
}

public sealed class NotifierDriver(NotifierDiscordApi api, Func<string, object, Task> dispatch)
{
    private long _id = 100000000000080000;

    public async Task<ScenarioMessage> MessageAsync(ScenarioGuild guild, ScenarioUser user, string content, int type = 0,
        string? attachmentUrl = null, ScenarioMessage? replyTo = null)
    {
        ScenarioMessage message = new($"{Interlocked.Increment(ref _id)}", guild, user, content, DateTimeOffset.UtcNow)
        {
            Type = type,
            AttachmentUrl = attachmentUrl,
            ReplyToId = replyTo?.Id,
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

    public async Task<DiscordOutput> EditAsync(ScenarioMessage message, string content)
    {
        var start = api.Requests.Count;
        await dispatch("MESSAGE_UPDATE", MessagePayload(message with { Content = content }));

        return new([.. api.Requests.Skip(start)]);
    }

    public async Task<DiscordOutput> RefreshEmbedsAsync(ScenarioMessage message)
    {
        var start = api.Requests.Count;
        var payload = MessagePayload(message);
        payload["embeds"] = JsonSerializer.SerializeToNode(new[] { new { type = "rich", description = "Link preview" } });

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
            author = NotifierDiscordApi.User(message.User.Id, message.User.Username, bot: message.User.Id == NotifierDiscordApi.BotId),
            content = message.Content,
            timestamp = message.Timestamp,
            edited_timestamp = (string?)null,
            type = message.Type,
            tts = false,
            pinned = false,
            mention_everyone = false,
            mentions = Array.Empty<object>(),
            mention_roles = Array.Empty<string>(),
            message_reference = message.ReplyToId == null ? null : new { message_id = message.ReplyToId, channel_id = message.Guild.ChannelId, guild_id = message.Guild.Id },
            attachments = message.AttachmentUrl == null ? [] : new[]
        {
            new { id = "100000000000070050", filename = "photo.png", size = 1, url = message.AttachmentUrl, proxy_url = message.AttachmentUrl, content_type = "image/png", width = 1, height = 1 },
        },
            embeds = Array.Empty<object>(),
            components = Array.Empty<object>(),
            flags = 0,
        })!.AsObject();

        DiscordMessageJson.RemoveNullProperties(payload);

        return payload;
    }
}
