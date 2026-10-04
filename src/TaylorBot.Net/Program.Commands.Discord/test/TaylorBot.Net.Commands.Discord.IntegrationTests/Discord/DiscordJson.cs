using System.Text.Json.Nodes;

using System.Text.Json;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Discord;

internal static class DiscordJson
{
    // Drop unset request fields before projecting the server's message representation.
    public static void RemoveNullProperties(JsonNode node)
    {
        if (node is JsonObject obj)
        {
            foreach (var property in obj.ToArray())
            {
                if (property.Value == null)
                {
                    obj.Remove(property.Key);
                }
                else
                {
                    RemoveNullProperties(property.Value);
                }
            }
        }
        else if (node is JsonArray array)
        {
            foreach (var child in array.OfType<JsonNode>())
            {
                RemoveNullProperties(child);
            }
        }
    }

    public static JsonObject CreateMessage(JsonObject content, string channelId, int type, JsonObject? previous = null,
        IReadOnlyList<DiscordAttachment>? uploads = null)
    {
        var message = previous?.DeepClone().AsObject() ?? [];
        var accepted = content.DeepClone().AsObject();
        RemoveNullProperties(accepted);
        foreach (var (name, value) in accepted)
        {
            if (name is "content" or "embeds" or "components" or "attachments" or "flags" or "tts" or "message_reference")
            {
                message[name] = value?.DeepClone();
            }
        }
        message["id"] = DiscordApi.ResponseMessageId;
        message["channel_id"] = channelId;
        message["type"] = type;
        message["author"] = JsonSerializer.SerializeToNode(DiscordDriver.UserPayload(new(DiscordApi.ApplicationId, "IntegrationBot")));
        message["timestamp"] ??= DateTimeOffset.UtcNow;
        message["edited_timestamp"] = previous == null ? null : JsonSerializer.SerializeToNode(DateTimeOffset.UtcNow);
        message["content"] ??= "";
        message["tts"] ??= false;
        message["pinned"] ??= false;
        message["mention_everyone"] ??= false;
        message["mentions"] ??= new JsonArray();
        message["mention_roles"] ??= new JsonArray();
        message["embeds"] ??= new JsonArray();
        message["attachments"] ??= new JsonArray();
        message["components"] ??= new JsonArray();
        message["flags"] ??= 0;
        if (uploads is { Count: > 0 })
        {
            message["attachments"] = new JsonArray([.. uploads.Select((upload, index) =>
            {
                var id = $"{100000000000000050 + index}";
                var url = $"https://cdn.discord.invalid/attachments/{channelId}/{id}/{Uri.EscapeDataString(upload.Name)}";
                return new JsonObject
                {
                    ["id"] = id,
                    ["filename"] = upload.Name,
                    ["size"] = upload.Bytes.Count,
                    ["url"] = url,
                    ["proxy_url"] = url,
                };
            })]);
        }
        AssignComponentIds(message["components"]!.AsArray());
        return message;
    }

    public static void AssignComponentIds(JsonArray components)
    {
        var id = 0;
        Assign(components);

        void Assign(JsonArray children)
        {
            foreach (var component in children.OfType<JsonObject>())
            {
                component["id"] = ++id;
                if (component["components"] is JsonArray nested)
                {
                    Assign(nested);
                }
            }
        }
    }
}
