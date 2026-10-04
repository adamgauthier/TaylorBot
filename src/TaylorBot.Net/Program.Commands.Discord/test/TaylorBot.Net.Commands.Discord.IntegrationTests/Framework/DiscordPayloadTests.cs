using System.Text.Json;
using System.Text.Json.Nodes;
using FluentAssertions;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Discord;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Scenarios;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Framework;

public sealed class DiscordPayloadTests
{
    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task GuildInteractions_UseMemberInsteadOfTopLevelUser(int type)
    {
        WireScenario scenario = new();

        await scenario.InteractAsync(type, inGuild: true);
        var payload = scenario.Incoming;

        payload.ContainsKey("user").Should().BeFalse();
        payload["member"]!["user"]!["id"]!.GetValue<string>().Should().Be(scenario.User.Id);
        payload["member"]!["roles"]!.AsArray().Select(role => role!.GetValue<string>()).Should().ContainSingle().Which.Should().Be(scenario.Role.Id);
        payload["guild_locale"]!.GetValue<string>().Should().Be("en-US");
        payload["context"]!.GetValue<int>().Should().Be(0);
        payload["authorizing_integration_owners"]!["0"]!.GetValue<string>().Should().Be(scenario.Guild.Id);
        payload["attachment_size_limit"]!.GetValue<int>().Should().BePositive();
        payload["channel"]!["guild_id"]!.GetValue<string>().Should().Be(scenario.Guild.Id);
        payload["channel"]!.AsObject().ContainsKey("recipients").Should().BeFalse();
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task BotDmInteractions_UseUserAndDmInstallationContext(int type)
    {
        WireScenario scenario = new();

        await scenario.InteractAsync(type, inGuild: false);
        var payload = scenario.Incoming;

        payload.ContainsKey("member").Should().BeFalse();
        payload.ContainsKey("guild_id").Should().BeFalse();
        payload["user"]!["id"]!.GetValue<string>().Should().Be(scenario.User.Id);
        payload["user"]!["discriminator"]!.GetValue<string>().Should().Be("0");
        payload["context"]!.GetValue<int>().Should().Be(1);
        payload["authorizing_integration_owners"]!["0"]!.GetValue<string>().Should().Be("0");
        payload["app_permissions"]!.GetValue<string>().Should().Be("562949953863680");
        payload["channel"]!["recipients"]![0]!["id"]!.GetValue<string>().Should().Be(DiscordApi.ApplicationId);
    }

    [Fact]
    public async Task BotDmInteractions_CanIncludeBothInstallationOwners()
    {
        WireScenario scenario = new();
        ScenarioDm dm = new(new(DiscordApi.ApplicationId, "IntegrationBot"), scenario.User);

        await scenario.InteractAsync(type: 2, inGuild: false, dm);

        scenario.Incoming["context"]!.GetValue<int>().Should().Be(1);
        scenario.Incoming["authorizing_integration_owners"]!["0"]!.GetValue<string>().Should().Be("0");
        scenario.Incoming["authorizing_integration_owners"]!["1"]!.GetValue<string>().Should().Be(scenario.User.Id);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task PrivateChannelInteractions_PreserveUserInstallationAndRecipient(int type)
    {
        WireScenario scenario = new();
        ScenarioUser recipient = new("100000000000000105", "Bob");
        ScenarioDm dm = new(recipient, scenario.User);

        await scenario.InteractAsync(type, inGuild: false, dm);
        var payload = scenario.Incoming;

        payload["context"]!.GetValue<int>().Should().Be(2);
        payload.ContainsKey("guild_id").Should().BeFalse();
        payload.ContainsKey("member").Should().BeFalse();
        payload["user"]!["id"]!.GetValue<string>().Should().Be(scenario.User.Id);
        payload["channel"]!["type"]!.GetValue<int>().Should().Be(1);
        payload["channel"]!["recipients"]![0]!["id"]!.GetValue<string>().Should().Be(recipient.Id);
        payload["authorizing_integration_owners"]!.AsObject().Should().ContainSingle();
        payload["authorizing_integration_owners"]!["1"]!.GetValue<string>().Should().Be(scenario.User.Id);
        payload["app_permissions"]!.GetValue<string>().Should().Be("562949953601536");
        scenario.Response["channel_id"]!.GetValue<string>().Should().Be(payload["channel_id"]!.GetValue<string>());
        scenario.Response["interaction_metadata"]!["authorizing_integration_owners"]!.ToJsonString()
            .Should().Be(payload["authorizing_integration_owners"]!.ToJsonString());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ResolvedGuildMembers_OmitFullMemberFields(bool useSelect)
    {
        WireScenario scenario = new();

        await scenario.SelectMemberAsync(useSelect);
        var resolved = scenario.Incoming["data"]!["resolved"]!;
        var member = resolved["members"]![scenario.User.Id]!.AsObject();

        resolved["users"]![scenario.User.Id]!["id"]!.GetValue<string>().Should().Be(scenario.User.Id);
        member.ContainsKey("user").Should().BeFalse();
        member.ContainsKey("deaf").Should().BeFalse();
        member.ContainsKey("mute").Should().BeFalse();
        member["permissions"]!.GetValue<string>().Should().Be("8");
        member["roles"]!.AsArray().Select(role => role!.GetValue<string>()).Should().ContainSingle().Which.Should().Be(scenario.Role.Id);
    }

    [Fact]
    public async Task ParameterlessCommands_OmitOptionsAndResolvedObjects()
    {
        WireScenario scenario = new();

        await scenario.InteractAsync(type: 2, inGuild: true);

        scenario.Incoming["data"]!.AsObject().ContainsKey("options").Should().BeFalse();
        scenario.Incoming["data"]!.AsObject().ContainsKey("resolved").Should().BeFalse();
    }

    [Fact]
    public async Task ComponentUpdates_PreserveOriginalMessageMetadataAndChannel()
    {
        WireScenario scenario = new();
        var command = await scenario.Driver.InvokeSlashCommandAsync(scenario.User, "help", scenario.Guild);
        var originalMetadata = scenario.Response["interaction_metadata"]!.ToJsonString();
        var update = await scenario.Driver.SelectAsync(scenario.User, command, "category");

        await scenario.Driver.SelectAsync(scenario.User, update, "category");
        var message = scenario.Incoming["message"]!;

        message["type"]!.GetValue<int>().Should().Be(20);
        message["channel_id"]!.GetValue<string>().Should().Be(scenario.Guild.ChannelId);
        message["webhook_id"]!.GetValue<string>().Should().Be(DiscordApi.ApplicationId);
        message["interaction_metadata"]!.ToJsonString().Should().Be(originalMetadata);
        scenario.Response["interaction_metadata"]!.ToJsonString().Should().Be(originalMetadata);
        scenario.Incoming["data"]!["id"]!.GetValue<int>().Should().Be(2);
        message["components"]![0]!["id"]!.GetValue<int>().Should().Be(1);
        message["components"]![0]!["components"]![0]!["id"]!.GetValue<int>().Should().Be(2);
    }

    [Fact]
    public async Task ModalSubmissions_IncludeComponentIdsAndTriggeringMetadata()
    {
        WireScenario scenario = new();

        await scenario.InteractAsync(type: 5, inGuild: true);
        var metadata = scenario.Response["interaction_metadata"]!;

        scenario.Incoming.ContainsKey("message").Should().BeFalse();
        scenario.Incoming["data"]!["components"]![0]!["id"]!.GetValue<int>().Should().Be(1);
        scenario.Incoming["data"]!["components"]![0]!["components"]![0]!["id"]!.GetValue<int>().Should().Be(2);
        metadata["type"]!.GetValue<int>().Should().Be(5);
        metadata["triggering_interaction_metadata"]!["type"]!.GetValue<int>().Should().Be(2);
        metadata["triggering_interaction_metadata"]!["name"]!.GetValue<string>().Should().Be("help");
    }

    [Fact]
    public async Task DeferredModalResponses_CreateNewMessagesInsteadOfEditingTheTrigger()
    {
        WireScenario scenario = new();
        var command = await scenario.Driver.InvokeSlashCommandAsync(scenario.User, "help", scenario.Guild);
        var modal = await scenario.OpenModalAsync(command);

        await scenario.Driver.SubmitModalAsync(scenario.User, modal, new Dictionary<string, string> { ["body"] = "synthetic text" });
        var metadata = scenario.Response["interaction_metadata"]!;

        scenario.Incoming["message"]!["interaction_metadata"]!["type"]!.GetValue<int>().Should().Be(2);
        metadata["type"]!.GetValue<int>().Should().Be(5);
        metadata["triggering_interaction_metadata"]!["type"]!.GetValue<int>().Should().Be(3);
        metadata["id"]!.GetValue<string>().Should().Be(scenario.Incoming["id"]!.GetValue<string>());
        scenario.Response["edited_timestamp"].Should().BeNull();
    }

    [Fact]
    public async Task LegacyReplies_UseReplyTypeAndRetainNullableMessageFields()
    {
        WireScenario scenario = new();

        await scenario.Driver.SendMessageAsync(scenario.User, scenario.Guild, "!help");

        scenario.Response["type"]!.GetValue<int>().Should().Be(19);
        scenario.Response["channel_id"]!.GetValue<string>().Should().Be(scenario.Guild.ChannelId);
        scenario.Response.ContainsKey("edited_timestamp").Should().BeTrue();
        scenario.Response["edited_timestamp"].Should().BeNull();
        scenario.Response.ContainsKey("interaction_metadata").Should().BeFalse();
        scenario.Response.ContainsKey("allowed_mentions").Should().BeFalse();
    }

    [Fact]
    public async Task UploadedFiles_ReturnAttachmentMetadataInsteadOfUploadIndexes()
    {
        WireScenario scenario = new();

        await scenario.UploadAsync();
        var attachment = scenario.Response["attachments"]!.AsArray().Should().ContainSingle().Which!;

        attachment["id"]!.GetValue<string>().Should().Be("100000000000000050");
        attachment["filename"]!.GetValue<string>().Should().Be("collage.png");
        attachment["size"]!.GetValue<int>().Should().Be(3);
        attachment["url"]!.GetValue<string>().Should().Be($"https://cdn.discord.invalid/attachments/{scenario.Guild.ChannelId}/100000000000000050/collage.png");
    }

    private sealed class WireScenario
    {
        private readonly DiscordApi _api = new();
        private bool _openModal;
        private DiscordAttachment? _upload;
        public DiscordDriver Driver { get; }
        public ScenarioUser User { get; } = new("100000000000000101", "Alice");
        public ScenarioRole Role { get; } = new("100000000000000102", "Regulars");
        public ScenarioGuild Guild { get; } = new("100000000000000103", "100000000000000104");
        public JsonObject Incoming { get; private set; } = null!;
        public JsonObject Response { get; private set; } = null!;

        public WireScenario()
        {
            Guild.Members.Add(User);
            Guild.MemberRoles[User.Id] = [Role.Id];
            Driver = new(_api, DispatchAsync);
        }

        public async Task InteractAsync(int type, bool inGuild, ScenarioDm? dm = null)
        {
            _openModal = type == 5;
            var command = await Driver.InvokeSlashCommandAsync(User, "help", inGuild ? Guild : null, dm: dm);
            if (type == 3)
            {
                await Driver.SelectAsync(User, command, "category");
            }
            else if (type == 5)
            {
                await Driver.SubmitModalAsync(User, command, new Dictionary<string, string> { ["body"] = "synthetic text" });
            }
        }

        public async Task SelectMemberAsync(bool useSelect)
        {
            if (useSelect)
            {
                var command = await Driver.InvokeSlashCommandAsync(User, "help", Guild);
                await Driver.SelectUserAsync(User, command, User);
            }
            else
            {
                await Driver.InvokeSlashCommandAsync(User, "inspect user", Guild, selectedUser: User);
            }
        }

        public Task<DiscordExchange> OpenModalAsync(DiscordExchange message)
        {
            _openModal = true;
            return Driver.ClickAsync(User, message, "Open modal");
        }

        public Task<DiscordExchange> UploadAsync()
        {
            _upload = new("collage.png", [1, 2, 3]);
            return Driver.InvokeSlashCommandAsync(User, "lastfm collage", Guild);
        }

        private Task DispatchAsync(string name, object payload)
        {
            Incoming = JsonSerializer.SerializeToNode(payload)!.AsObject();
            if (name == "MESSAGE_CREATE")
            {
                JsonObject content = new()
                {
                    ["content"] = "Synthetic reply",
                    ["message_reference"] = new JsonObject { ["message_id"] = Incoming["id"]!.DeepClone() },
                    ["allowed_mentions"] = new JsonObject { ["parse"] = new JsonArray() },
                };
                Response = JsonNode.Parse(_api.Send("POST", $"channels/{Guild.ChannelId}/messages", content.ToJsonString()).Body)!.AsObject();
                return Task.CompletedTask;
            }

            var id = Incoming["id"]!.GetValue<string>();
            var token = Incoming["token"]!.GetValue<string>();
            var callback = $"interactions/{id}/{token}/callback";
            if (_openModal)
            {
                _openModal = false;
                _api.Send("POST", callback, """
                    {"type":9,"data":{"custom_id":"synthetic-modal","title":"Test","components":[
                      {"type":1,"components":[{"type":4,"custom_id":"body","label":"Body","style":1}]}]}}
                    """);
                return Task.CompletedTask;
            }
            var update = Incoming["type"]!.GetValue<int>() is 3 or 5;
            var acknowledgement = Incoming["type"]!.GetValue<int>() == 3 ? """{"type":6}""" : """{"type":5}""";
            _api.Send("POST", callback, acknowledgement);
            var path = $"webhooks/{DiscordApi.ApplicationId}/{token}" + (update ? "/messages/@original" : "");
            var response = JsonNode.Parse("""
                {"embeds":[{"description":"Synthetic response"}],"components":[
                  {"type":1,"components":[{"type":3,"custom_id":"category-select","options":[{"label":"Category","value":"category"}]}]},
                  {"type":1,"components":[{"type":5,"custom_id":"user-select"}]},
                  {"type":1,"components":[{"type":2,"style":1,"custom_id":"modal-button","label":"Open modal"}]}]}
                """)!;
            if (_upload != null)
            {
                response["attachments"] = new JsonArray(new JsonObject { ["id"] = 0, ["filename"] = _upload.Name });
            }
            Response = JsonNode.Parse(_api.Send(update ? "PATCH" : "POST", path, response.ToJsonString(),
                attachments: _upload == null ? null : [_upload]).Body)!.AsObject();
            return Task.CompletedTask;
        }
    }
}
