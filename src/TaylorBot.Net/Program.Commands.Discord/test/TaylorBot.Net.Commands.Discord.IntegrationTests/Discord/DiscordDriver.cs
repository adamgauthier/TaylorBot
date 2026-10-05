using System.Text.Json;
using System.Text.Json.Nodes;
using FluentAssertions;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Scenarios;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Discord;

public sealed class DiscordDriver(DiscordApi api, Func<string, object, Task> dispatch)
{
    private long _id = 100000000000001000;

    internal static object UserPayload(ScenarioUser user) =>
        DiscordApiStub.User(user.Id, user.Username, bot: user.Id == DiscordApi.ApplicationId, avatar: user.Avatar);

    private static JsonObject CreateInteraction(string id, string token, int type, ScenarioUser user, ScenarioGuild? guild,
        string permissions = "8", ScenarioDm? dm = null)
    {
        if (guild != null && dm != null)
            throw new ArgumentException("An interaction cannot be in both a guild and a DM.", nameof(dm));

        var recipient = dm?.Recipient ?? new(DiscordApi.ApplicationId, "IntegrationBot");
        var isBotDm = recipient.Id == DiscordApi.ApplicationId;
        if (guild == null && !isBotDm && dm?.InstallationOwner == null)
            throw new ArgumentException("A private-channel interaction requires a user installation.", nameof(dm));

        JsonObject owners = [];
        if (guild != null || isBotDm)
            owners["0"] = guild?.Id ?? "0";
        if (dm?.InstallationOwner != null)
            owners["1"] = dm.InstallationOwner.Id;

        JsonObject channel = new()
        {
            ["id"] = guild?.ChannelId ?? (isBotDm ? "100000000000000004" : "100000000000000007"),
            ["type"] = guild == null ? 1 : 0,
        };
        JsonObject payload = new()
        {
            ["id"] = id,
            ["application_id"] = DiscordApi.ApplicationId,
            ["type"] = type,
            ["token"] = token,
            ["version"] = 1,
            ["app_permissions"] = guild != null ? "8" : isBotDm ? "562949953863680" : "562949953601536",
            ["entitlements"] = new JsonArray(),
            ["authorizing_integration_owners"] = owners,
            ["context"] = guild != null ? 0 : isBotDm ? 1 : 2,
            ["attachment_size_limit"] = 10485760,
            ["locale"] = "en-US",
            ["channel_id"] = channel["id"]!.DeepClone(),
            ["channel"] = channel,
        };

        if (guild == null)
        {
            channel["recipients"] = new JsonArray(JsonSerializer.SerializeToNode(UserPayload(recipient)));
            payload["user"] = JsonSerializer.SerializeToNode(UserPayload(user));
        }
        else
        {
            channel["guild_id"] = guild.Id;
            channel["name"] = "general";
            channel["permissions"] = permissions;
            payload["guild_id"] = guild.Id;
            payload["guild_locale"] = "en-US";
            payload["member"] = JsonSerializer.SerializeToNode(new
            {
                user = UserPayload(user),
                roles = guild.MemberRoles.GetValueOrDefault(user.Id, []),
                permissions,
                joined_at = "2026-01-01T00:00:00Z",
                deaf = false,
                mute = false,
            });
        }

        return payload;
    }

    private static void AddResolvedMembers(JsonObject resolved, ScenarioGuild? guild)
    {
        if (guild != null && resolved["users"] is JsonObject users)
        {
            JsonObject members = [];
            foreach (var member in guild.Members.Where(member => users.ContainsKey(member.Id)))
            {
                members[member.Id] = JsonSerializer.SerializeToNode(new
                {
                    roles = guild.MemberRoles.GetValueOrDefault(member.Id, []),
                    joined_at = "2026-01-01T00:00:00Z",
                    permissions = "8",
                });
            }

            if (members.Count != 0)
            {
                resolved["members"] = members;
            }
        }
    }

    public async Task<DiscordExchange> InvokeSlashCommandAsync(
        ScenarioUser user, string command, ScenarioGuild? guild = null, ScenarioUser? selectedUser = null,
        IReadOnlyList<SlashArgument>? arguments = null, string permissions = "8", ScenarioDm? dm = null)
    {
        var parts = command.Split(' ');
        var id = $"{Interlocked.Increment(ref _id)}";
        var token = $"interaction-{id}";

        JsonArray options = [];
        JsonObject resolved = [];
        foreach (var argument in arguments ?? [])
        {
            options.Add(new JsonObject { ["name"] = argument.Name, ["type"] = argument.Type, ["value"] = argument.Value.DeepClone() });
            if (argument.ResolvedCollection != null)
            {
                resolved[argument.ResolvedCollection] ??= new JsonObject();
                resolved[argument.ResolvedCollection]![argument.Value.GetValue<string>()] = argument.Resolved!.DeepClone();
            }
        }

        if (selectedUser != null)
        {
            options.Add(new JsonObject { ["name"] = "user", ["type"] = 6, ["value"] = selectedUser.Id });
        }

        var leafOptions = options;
        for (var index = parts.Length - 1; index > 0; index--)
        {
            options = new(new JsonObject
            {
                ["name"] = parts[index],
                ["type"] = index == parts.Length - 1 ? 1 : 2,
                ["options"] = options,
            });
        }

        var payload = CreateInteraction(id, token, type: 2, user, guild, permissions, dm);
        JsonObject data = new()
        {
            ["id"] = api.GetCommandId(parts[0], guild?.Id),
            ["name"] = parts[0],
            ["type"] = 1,
        };
        payload["data"] = data;
        if (options.Count != 0)
        {
            data["options"] = parts.Length == 1 ? leafOptions : options;
        }

        if (selectedUser != null)
        {
            resolved["users"] ??= new JsonObject();
            resolved["users"]![selectedUser.Id] = JsonSerializer.SerializeToNode(UserPayload(selectedUser));
        }

        AddResolvedMembers(resolved, guild);
        if (resolved.Count != 0)
        {
            data["resolved"] = resolved;
        }

        api.ExpectInteraction(payload);
        await dispatch("INTERACTION_CREATE", payload);

        return new(api.ForInteraction(id, token), user, guild, dm: dm);
    }

    public async Task<DiscordExchange> ClickAsync(ScenarioUser user, DiscordExchange previous, string label)
    {
        var button = previous.Message.GetProperty("components").EnumerateArray()
            .SelectMany(row => row.GetProperty("components").EnumerateArray())
            .Single(component => component.GetProperty("type").GetInt32() == 2
                && component.GetProperty("label").GetString() == label);
        return await InvokeComponentAsync(user, previous, button, null);
    }

    public async Task<DiscordExchange> SelectUserAsync(ScenarioUser user, DiscordExchange previous, ScenarioUser selected)
    {
        var select = previous.Message.GetProperty("components").EnumerateArray()
            .SelectMany(row => row.GetProperty("components").EnumerateArray())
            .Single(component => component.GetProperty("type").GetInt32() == 5);
        return await InvokeComponentAsync(user, previous, select, SlashArgument.User("selection", selected));
    }

    public Task<DiscordExchange> SelectAsync(ScenarioUser user, DiscordExchange previous, string value) =>
        InvokeComponentAsync(user, previous, FindSelect(previous, type: 3), SlashArgument.Text("selection", value));

    public Task<DiscordExchange> SelectRoleAsync(ScenarioUser user, DiscordExchange previous, ScenarioRole role) =>
        InvokeComponentAsync(user, previous, FindSelect(previous, type: 6), SlashArgument.Role("selection", role));

    public Task<DiscordExchange> SelectChannelAsync(ScenarioUser user, DiscordExchange previous, ScenarioGuild guild) =>
        InvokeComponentAsync(user, previous, FindSelect(previous, type: 8), SlashArgument.Channel("selection", guild));

    private static JsonElement FindSelect(DiscordExchange previous, int type) =>
        previous.Message.GetProperty("components").EnumerateArray()
            .SelectMany(row => row.GetProperty("components").EnumerateArray())
            .Single(component => component.GetProperty("type").GetInt32() == type);

    public async Task<DiscordExchange> SubmitModalAsync(
        ScenarioUser user, DiscordExchange previous, IReadOnlyDictionary<string, string> fields)
    {
        var modal = previous.Modal;
        var inputs = modal.GetProperty("components").EnumerateArray()
            .SelectMany(row => row.GetProperty("components").EnumerateArray()).ToArray();
        var id = $"{Interlocked.Increment(ref _id)}";
        var token = $"interaction-{id}";
        var payload = CreateInteraction(id, token, type: 5, user, previous.Guild, dm: previous.Dm);
        payload["data"] = new JsonObject
        {
            ["custom_id"] = modal.GetProperty("custom_id").GetString(),
            ["components"] = new JsonArray([.. fields.Select(field =>
            {
                var input = inputs.Single(input => input.GetProperty("custom_id").GetString() == field.Key
                    || input.GetProperty("label").GetString() == field.Key);
                return new JsonObject
                {
                    ["type"] = 1,
                    ["components"] = new JsonArray(new JsonObject
                    {
                        ["type"] = 4,
                        ["custom_id"] = input.GetProperty("custom_id").GetString(),
                        ["value"] = field.Value,
                    }),
                };
            })]),
        };

        DiscordMessageJson.AssignComponentIds(payload["data"]!["components"]!.AsArray());
        if (previous.OriginMessage is { } message)
        {
            payload["message"] = JsonNode.Parse(message.GetRawText());
        }

        api.ExpectInteraction(payload, modalTrigger: previous);
        await dispatch("INTERACTION_CREATE", payload);

        return new(api.ForInteraction(id, token), previous.Author, previous.Guild, expectedEditAcknowledgement: 5, dm: previous.Dm);
    }

    private async Task<DiscordExchange> InvokeComponentAsync(ScenarioUser user, DiscordExchange previous, JsonElement component, SlashArgument? selected)
    {
        var id = $"{Interlocked.Increment(ref _id)}";
        var token = $"interaction-{id}";
        var message = api.GetResponseMessage(previous.Requests[^1]);
        var receivedComponent = message["components"]!.AsArray()
            .SelectMany(row => row!["components"]!.AsArray())
            .Single(item => item!["custom_id"]?.GetValue<string>() == component.GetProperty("custom_id").GetString())!;

        var payload = CreateInteraction(id, token, type: 3, user, previous.Guild, dm: previous.Dm);
        payload["message"] = message;
        payload["data"] = new JsonObject
        {
            ["id"] = receivedComponent["id"]!.DeepClone(),
            ["custom_id"] = component.GetProperty("custom_id").GetString(),
            ["component_type"] = component.GetProperty("type").GetInt32(),
        };

        if (selected != null)
        {
            payload["data"]!["values"] = new JsonArray(selected.Value.DeepClone());
            if (selected.ResolvedCollection != null)
            {
                payload["data"]!["resolved"] = new JsonObject
                {
                    [selected.ResolvedCollection] = new JsonObject { [selected.Value.GetValue<string>()] = selected.Resolved!.DeepClone() },
                };
                AddResolvedMembers(payload["data"]!["resolved"]!.AsObject(), previous.Guild);
            }
        }

        api.ExpectInteraction(payload);
        await dispatch("INTERACTION_CREATE", payload);

        return new(api.ForInteraction(id, token), previous.Author, previous.Guild, JsonSerializer.SerializeToElement(message), dm: previous.Dm);
    }

    public async Task<DiscordExchange> SendMessageAsync(ScenarioUser user, ScenarioGuild guild, string content)
    {
        var id = $"{Interlocked.Increment(ref _id)}";
        api.ExpectMessage(guild.ChannelId, id);
        await dispatch("MESSAGE_CREATE", new
        {
            id,
            channel_id = guild.ChannelId,
            guild_id = guild.Id,
            content,
            author = UserPayload(user),
            member = new { roles = guild.MemberRoles.GetValueOrDefault(user.Id, []), joined_at = "2026-01-01T00:00:00Z", deaf = false, mute = false },
            timestamp = DateTimeOffset.UtcNow,
            edited_timestamp = (string?)null,
            tts = false,
            mention_everyone = false,
            mentions = guild.Members.Where(member => content.Contains($"<@{member.Id}>", StringComparison.Ordinal)
                || content.Contains($"<@!{member.Id}>", StringComparison.Ordinal)).Select(UserPayload).ToArray(),
            mention_roles = Array.Empty<string>(),
            attachments = Array.Empty<object>(),
            embeds = Array.Empty<object>(),
            pinned = false,
            type = 0,
        });

        return new(api.ForMessage(guild.ChannelId, id), user, guild);
    }
}

public sealed class DiscordExchange(IReadOnlyList<DiscordRequest> requests, ScenarioUser? author = null, ScenarioGuild? guild = null,
    JsonElement? originMessage = null, int expectedEditAcknowledgement = 6, ScenarioDm? dm = null)
{
    public IReadOnlyList<DiscordRequest> Requests { get; } = requests;
    internal ScenarioUser? Author { get; } = author;
    internal ScenarioGuild? Guild { get; } = guild;
    internal ScenarioDm? Dm { get; } = dm;
    internal JsonElement? OriginMessage { get; } = originMessage;

    public JsonElement Modal
    {
        get
        {
            var request = Requests.Should().ContainSingle().Which;
            request.Method.Should().Be("POST");
            request.Path.Should().EndWith("/callback");
            request.Body!.Value.GetProperty("type").GetInt32().Should().Be(9);

            return request.Body.Value.GetProperty("data");
        }
    }

    public JsonElement Message
    {
        get
        {
            Requests.Should().NotBeEmpty("the command must produce a response");
            if (Requests.Count == 1 && Requests[0].Path.EndsWith("/callback", StringComparison.Ordinal))
            {
                Requests[0].Method.Should().Be("POST");
                Requests[0].Body!.Value.GetProperty("type").GetInt32().Should().BeOneOf(4, 7);
                return Requests[0].Body!.Value.GetProperty("data");
            }

            if (Requests[0].Path.StartsWith("channels/", StringComparison.Ordinal))
            {
                Requests.Should().ContainSingle();
                Requests[0].Method.Should().BeOneOf("POST", "PATCH");
                return Requests[0].Body!.Value;
            }

            Requests.Should().HaveCount(2);
            if (Requests[1].Method == "PATCH")
            {
                Requests[0].Method.Should().Be("POST");
                Requests[0].Path.Should().EndWith("/callback");
                Requests[0].Body!.Value.GetProperty("type").GetInt32().Should().Be(expectedEditAcknowledgement);
                Requests[1].Path.Should().StartWith($"webhooks/{DiscordApi.ApplicationId}/").And.EndWith("/messages/@original");
                return Requests[1].Body!.Value;
            }

            return ShouldBeDeferredMessage();
        }
    }

    public JsonElement Embed => Message.GetProperty("embeds").EnumerateArray().Should().ContainSingle().Which;
    public string Description => Embed.GetProperty("description").GetString()!;
    public string Field(string name) => Embed.GetProperty("fields").EnumerateArray()
        .Single(field => field.GetProperty("name").GetString() == name).GetProperty("value").GetString()!;
    public void ShouldBeSuccess() => Embed.GetProperty("color").GetInt32().Should().Be(0x43b581);
    public void ShouldBeError() => Embed.GetProperty("color").GetInt32().Should().Be(0xf04747);

    public void ShouldBeDeleted()
    {
        Requests.Should().HaveCount(2);
        Requests[0].Method.Should().Be("POST");
        Requests[0].Path.Should().EndWith("/callback");
        Requests[0].Body!.Value.GetProperty("type").GetInt32().Should().Be(6);
        Requests[1].Method.Should().Be("DELETE");
        Requests[1].Path.Should().StartWith($"webhooks/{DiscordApi.ApplicationId}/").And.EndWith("/messages/@original");
    }

    public JsonElement ShouldBeDeferredMessage()
    {
        Requests.Should().HaveCount(2);
        Requests[0].Method.Should().Be("POST");
        Requests[0].Path.Should().EndWith("/callback");
        Requests[0].Body!.Value.GetProperty("type").GetInt32().Should().Be(5);
        Requests[1].Method.Should().Be("POST");
        Requests[1].Path.Should().StartWith($"webhooks/{DiscordApi.ApplicationId}/");

        return Requests[1].Body!.Value;
    }

    public string ShouldHaveSingleEmbedDescription()
    {
        var response = ShouldBeDeferredMessage();
        var embeds = response.GetProperty("embeds").EnumerateArray().ToArray();
        return embeds.Should().ContainSingle().Which.GetProperty("description").GetString()
            ?? throw new InvalidOperationException("The response embed has no description.");
    }
}
