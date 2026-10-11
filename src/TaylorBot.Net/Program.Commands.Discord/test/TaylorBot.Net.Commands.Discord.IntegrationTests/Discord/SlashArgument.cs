using System.Text.Json.Nodes;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Scenarios;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Discord;

public sealed record SlashArgument(string Name, int Type, JsonNode Value, string? ResolvedCollection = null, JsonNode? Resolved = null)
{
    public static SlashArgument Text(string name, string value) => new(name, Type: 3, JsonValue.Create(value));
    public static SlashArgument Integer(string name, long value) => new(name, Type: 4, JsonValue.Create(value));
    public static SlashArgument Boolean(string name, bool value) => new(name, Type: 5, JsonValue.Create(value));

    public static SlashArgument Attachment(string name, string url) =>
        new(name, Type: 11, JsonValue.Create("100000000000000050"), "attachments", new JsonObject
        {
            ["id"] = "100000000000000050",
            ["filename"] = Path.GetFileName(new Uri(url).AbsolutePath),
            ["size"] = 4,
            ["url"] = url,
            ["proxy_url"] = url,
            ["content_type"] = "image/png",
            ["ephemeral"] = true,
        });

    public static SlashArgument User(string name, ScenarioUser user) =>
        new(name, Type: 6, JsonValue.Create(user.Id), "users", System.Text.Json.JsonSerializer.SerializeToNode(DiscordDriver.UserPayload(user)));

    public static SlashArgument Channel(string name, ScenarioGuild guild, ChannelDetails? details = null) =>
        new(name, Type: 7, JsonValue.Create(guild.ChannelId), "channels", (details ?? new(Name: "general")).Payload(guild));

    public static SlashArgument Role(string name, ScenarioRole role) =>
        new(name, Type: 8, JsonValue.Create(role.Id), "roles", role.Payload());
}

public sealed record ChannelDetails(string? Name = null, int Type = 0, string? Topic = null, bool IncludeTopic = false,
    string? ParentId = null, bool IncludeParent = false, bool? Nsfw = null)
{
    public JsonObject Payload(ScenarioGuild guild)
    {
        JsonObject payload = new()
        {
            ["id"] = guild.ChannelId,
            ["type"] = Type,
            ["permissions"] = "8",
            ["app_permissions"] = "8",
            ["guild_id"] = guild.Id,
        };
        if (Name != null)
            payload["name"] = Name;
        if (IncludeTopic)
            payload["topic"] = Topic;
        if (IncludeParent)
            payload["parent_id"] = ParentId;
        if (Nsfw.HasValue)
            payload["nsfw"] = Nsfw.Value;
        return payload;
    }
}
