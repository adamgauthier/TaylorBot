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
    public static SlashArgument Channel(string name, ScenarioGuild guild) =>
        new(name, Type: 7, JsonValue.Create(guild.ChannelId), "channels", new JsonObject
        {
            ["id"] = guild.ChannelId,
            ["name"] = "general",
            ["type"] = 0,
            ["permissions"] = "8",
            ["app_permissions"] = "8",
            ["guild_id"] = guild.Id,
        });
    public static SlashArgument Role(string name, ScenarioRole role) =>
        new(name, Type: 8, JsonValue.Create(role.Id), "roles", role.Payload());
}
