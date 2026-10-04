using System.Net;
using System.Text.Json.Nodes;

namespace TaylorBot.Net.UserNotifier.IntegrationTests.Discord;

public sealed class NotifierDiscordApiTests
{
    [Fact]
    public void Notifications_UseSharedMessageShapesAndDistinctIds()
    {
        NotifierDiscordApi api = new();
        api.ExpectMessage("channel");
        api.ExpectMessage("channel");

        var first = JsonNode.Parse(api.Send("POST", "channels/channel/messages?with_components=true",
            """{"content":"Hello","allowed_mentions":{"parse":[]},"embeds":[{"description":"News","url":null}]}""").Body)!.AsObject();
        var second = JsonNode.Parse(api.Send("POST", "channels/channel/messages", """{"content":"Again"}""").Body)!;

        first["id"]!.GetValue<string>().Should().NotBe(second["id"]!.GetValue<string>());
        first["channel_id"]!.GetValue<string>().Should().Be("channel");
        first["type"]!.GetValue<int>().Should().Be(0);
        first["author"]!["id"]!.GetValue<string>().Should().Be(NotifierDiscordApi.BotId);
        first["content"]!.GetValue<string>().Should().Be("Hello");
        first.ContainsKey("allowed_mentions").Should().BeFalse();
        first["embeds"]![0]!.AsObject().ContainsKey("url").Should().BeFalse();
        first.ContainsKey("edited_timestamp").Should().BeTrue();
        first["edited_timestamp"].Should().BeNull();
        api.Requests.Should().HaveCount(2);
        api.Requests[0].Body!.Value.TryGetProperty("allowed_mentions", out _).Should().BeTrue();
        api.EnsureExpectationsMet();
    }

    [Fact]
    public void DmCreation_UsesTheRegisteredRecipient()
    {
        NotifierDiscordApi api = new();
        const string UserId = "100000000000000101";
        api.Resource($"users/{UserId}", NotifierDiscordApi.User(UserId, "Alice"));

        var response = api.Send("POST", "users/@me/channels", $$"""{"recipient_id":"{{UserId}}"}""");
        var channel = JsonNode.Parse(response.Body)!;

        response.Status.Should().Be(HttpStatusCode.OK);
        channel["id"]!.GetValue<string>().Should().Be(NotifierDiscordApi.DmChannel(UserId));
        channel["recipients"]![0]!["username"]!.GetValue<string>().Should().Be("Alice");
        api.EnsureExpectationsMet();
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.Forbidden)]
    public void DmCreation_DoesNotTurnFailedUserLookupsIntoSuccess(HttpStatusCode status)
    {
        NotifierDiscordApi api = new();
        api.Resource("users/100000000000000101", new { code = 10013 }, status);

        var request = () => api.Send("POST", "users/@me/channels", """{"recipient_id":"100000000000000101"}""");

        request.Should().Throw<InvalidOperationException>().WithMessage("*Unexpected Discord request*");
        var verify = api.EnsureNoUnexpectedRequests;
        verify.Should().Throw<InvalidOperationException>().WithMessage("*Unexpected Discord request*");
    }

    [Fact]
    public void Resources_ReturnIndependentPayloadsUntilExplicitlyUpdated()
    {
        NotifierDiscordApi api = new();
        api.Resource("guilds/guild", new { name = "Original" });
        var updated = api.GetResource("guilds/guild");
        updated["name"] = "Changed";

        var before = api.GetResource("guilds/guild");
        api.Resource("guilds/guild", updated);
        var after = JsonNode.Parse(api.Send("GET", "guilds/guild").Body)!;

        before["name"]!.GetValue<string>().Should().Be("Original");
        after["name"]!.GetValue<string>().Should().Be("Changed");
        api.EnsureExpectationsMet();
    }

    [Fact]
    public void Expectations_PreserveNotifierResponseEvaluationAtRequestTime()
    {
        NotifierDiscordApi api = new();
        JsonObject payload = new() { ["content"] = "Original" };
        api.Expect("POST", "channels/channel/messages", payload, HttpStatusCode.OK);
        payload["content"] = "Changed";

        var response = api.Send("POST", "channels/channel/messages", "{}");

        JsonNode.Parse(response.Body)!["content"]!.GetValue<string>().Should().Be("Changed");
        api.EnsureExpectationsMet();
    }
}
