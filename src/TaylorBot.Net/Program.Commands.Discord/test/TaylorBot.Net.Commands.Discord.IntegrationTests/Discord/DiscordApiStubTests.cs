using System.Net;
using System.Text.Json.Nodes;
using FluentAssertions;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Discord;

public sealed class DiscordApiStubTests
{
    [Theory]
    [InlineData("gateway/bot", "url", "wss://gateway.invalid")]
    [InlineData("users/@me", "id", DiscordApiStub.ApplicationId)]
    [InlineData("oauth2/applications/@me", "id", DiscordApiStub.ApplicationId)]
    public void StartupResponses_AreAvailableWithoutExpectations(string path, string property, string value)
    {
        DiscordApiStub api = new();

        var response = api.Send("GET", path);

        response.Status.Should().Be(HttpStatusCode.OK);
        JsonNode.Parse(response.Body)![property]!.GetValue<string>().Should().Be(value);
        api.EnsureExpectationsMet();
    }

    [Fact]
    public void Expectations_AreConsumedInOrderAndCannotBeReused()
    {
        DiscordApiStub api = new();
        api.Expect("POST", "/channels/channel/messages", new { content = "First" }, HttpStatusCode.OK);
        api.Expect("POST", "channels/channel/messages", new { content = "Second" }, HttpStatusCode.Forbidden);

        var first = api.Send("POST", "channels/channel/messages", "{}");
        var second = api.Send("POST", "/channels/channel/messages", "{}");
        var extra = () => api.Send("POST", "channels/channel/messages", "{}");

        first.Status.Should().Be(HttpStatusCode.OK);
        JsonNode.Parse(first.Body)!["content"]!.GetValue<string>().Should().Be("First");
        second.Status.Should().Be(HttpStatusCode.Forbidden);
        JsonNode.Parse(second.Body)!["content"]!.GetValue<string>().Should().Be("Second");
        api.EnsureExpectationsMet();
        extra.Should().Throw<InvalidOperationException>().WithMessage("*Unexpected Discord request*");
        api.Requests.Should().HaveCount(3);
        var verify = api.EnsureExpectationsMet;
        verify.Should().Throw<InvalidOperationException>().WithMessage("*Unexpected Discord request*");
    }

    [Fact]
    public void UnusedExpectations_ReportTheRemainingCount()
    {
        DiscordApiStub api = new();
        api.Expect("DELETE", "channels/channel/messages/message");
        api.Expect("DELETE", "channels/channel/messages/message");
        api.Send("DELETE", "channels/channel/messages/message");

        var verify = api.EnsureExpectationsMet;

        verify.Should().Throw<InvalidOperationException>().WithMessage("*DELETE channels/channel/messages/message (1 remaining)*");
    }

    [Fact]
    public void RequestCapture_PreservesTheBodyAndUploadsWithoutExposingTheLiveList()
    {
        DiscordApiStub api = new();
        DiscordAttachment upload = new("photo.png", [1, 2, 3]);
        api.Expect("POST", "channels/channel/messages", request => (HttpStatusCode.OK, request.Body!.Value.GetRawText()));
        var before = api.Requests;

        var response = api.Send("POST", "/channels/channel/messages", """{"content":"Hello"}""", [upload]);

        before.Should().BeEmpty();
        var request = api.Requests.Should().ContainSingle().Which;
        request.Method.Should().Be("POST");
        request.Path.Should().Be("channels/channel/messages");
        request.Body!.Value.GetProperty("content").GetString().Should().Be("Hello");
        request.Attachments.Should().ContainSingle().Which.Should().Be(upload);
        JsonNode.Parse(response.Body)!["content"]!.GetValue<string>().Should().Be("Hello");
        api.EnsureExpectationsMet();
    }

    [Fact]
    public void Expectations_MatchTheQueryStringButCaptureOnlyThePath()
    {
        DiscordApiStub api = new();
        api.Expect("GET", "channels/channel/messages?limit=1", new { content = "Matched" }, HttpStatusCode.OK);

        var response = api.Send("GET", "/channels/channel/messages?limit=1");

        response.Status.Should().Be(HttpStatusCode.OK);
        api.Requests.Should().ContainSingle().Which.Path.Should().Be("channels/channel/messages");
        api.EnsureExpectationsMet();
    }

    [Fact]
    public void Expectations_RejectADifferentQueryString()
    {
        DiscordApiStub api = new();
        api.Expect("GET", "channels/channel/messages?limit=1");

        var request = () => api.Send("GET", "channels/channel/messages?limit=2");

        request.Should().Throw<InvalidOperationException>().WithMessage("*limit=2*");
    }

    [Fact]
    public void ObjectExpectations_CaptureTheirResponseAtRegistration()
    {
        DiscordApiStub api = new();
        JsonObject payload = new() { ["content"] = "Original" };
        api.Expect("POST", "channels/channel/messages", payload, HttpStatusCode.OK);
        payload["content"] = "Changed";

        var response = api.Send("POST", "channels/channel/messages", "{}");

        JsonNode.Parse(response.Body)!["content"]!.GetValue<string>().Should().Be("Original");
        api.EnsureExpectationsMet();
    }

    [Fact]
    public void Resources_AreEvaluatedAtRequestTimeAndCanBeReplaced()
    {
        DiscordApiStub api = new();
        var username = "Alice";
        api.Resource("users/user", () => new { username });
        username = "Bob";

        var current = api.Send("GET", "users/user?unused=true");
        api.Resource("users/user", new { code = 10013 }, HttpStatusCode.NotFound);
        var removed = api.Send("GET", "users/user");

        JsonNode.Parse(current.Body)!["username"]!.GetValue<string>().Should().Be("Bob");
        removed.Status.Should().Be(HttpStatusCode.NotFound);
        JsonNode.Parse(removed.Body)!["code"]!.GetValue<int>().Should().Be(10013);
        api.EnsureExpectationsMet();
    }

    [Fact]
    public void Expectations_OverrideResourcesAndStartupResponses()
    {
        DiscordApiStub api = new();
        api.Resource("users/@me", new { username = "Resource" });
        api.Expect("GET", "users/@me", new { username = "Expected" }, HttpStatusCode.Forbidden);

        var expected = api.Send("GET", "users/@me");
        var resource = api.Send("GET", "users/@me");

        expected.Status.Should().Be(HttpStatusCode.Forbidden);
        JsonNode.Parse(expected.Body)!["username"]!.GetValue<string>().Should().Be("Expected");
        JsonNode.Parse(resource.Body)!["username"]!.GetValue<string>().Should().Be("Resource");
        api.EnsureExpectationsMet();
    }

    [Fact]
    public void ApplicationResponses_RunUnderTheSharedLockAndRemainStrict()
    {
        Lock sync = new();
        DiscordApiStub api = new(sync, request =>
        {
            sync.IsHeldByCurrentThread.Should().BeTrue();
            return request.Path == "application-route" ? (HttpStatusCode.OK, "{}") : null;
        });
        api.Expect("GET", "application-route", status: HttpStatusCode.NoContent);

        var expected = api.Send("GET", "application-route");
        var application = api.Send("GET", "application-route");
        var unexpected = () => api.Send("GET", "unknown-route");

        expected.Status.Should().Be(HttpStatusCode.NoContent);
        application.Status.Should().Be(HttpStatusCode.OK);
        sync.IsHeldByCurrentThread.Should().BeFalse();
        unexpected.Should().Throw<InvalidOperationException>().WithMessage("*unknown-route*");
        var verify = api.EnsureNoUnexpectedRequests;
        verify.Should().Throw<InvalidOperationException>().WithMessage("*unknown-route*");
    }
}
