using System.Net;
using FluentAssertions;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.ExternalApis;

public sealed class ExternalApiTests
{
    [Fact]
    public async Task UnknownRequest_RemainsFailureAfterCallerCatchesIt()
    {
        ExternalApi api = new();
        using HttpRequestMessage request = new(HttpMethod.Get, "https://example.invalid/unconfigured");

        var send = async () =>
        {
            using var response = await api.SendAsync(request, TestContext.Current.CancellationToken);
        };

        await send.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Unexpected external request*");
        api.Invoking(instance => instance.EnsureNoUnexpectedRequests()).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public async Task ConfiguredResponse_PreservesStatusAndRecordsRequestBody()
    {
        ExternalApi api = new();
        api.Json("POST", "https://example.invalid/search?a=1&b=2", new { error = "Synthetic rejection" }, HttpStatusCode.Forbidden);
        using HttpRequestMessage request = new(HttpMethod.Post, "https://example.invalid/search?b=2&a=1")
        {
            Content = new StringContent("""{"query":"synthetic"}"""),
        };

        using var response = await api.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        api.Requests.Should().ContainSingle().Which.Body.Should().Be("""{"query":"synthetic"}""");
        api.EnsureExpectationsMet();
    }

    [Fact]
    public void UnusedResponse_FailsVerification()
    {
        ExternalApi api = new();
        api.Json("GET", "https://example.invalid/expected", new { value = 13 });

        var verify = () => api.EnsureExpectationsMet();

        verify.Should().Throw<InvalidOperationException>().WithMessage("*Expected external requests were not made*");
    }

    [Theory]
    [InlineData("https://example.invalid/search?user=wrong&nonce=123")]
    [InlineData("https://example.invalid/search?user=alice&nonce=invalid")]
    [InlineData("https://example.invalid/search?user=alice")]
    [InlineData("https://example.invalid/search?user=alice&nonce=123&extra=true")]
    public async Task NumericQueryAllowance_DoesNotWeakenOtherMatching(string uri)
    {
        ExternalApi api = new();
        api.Json("GET", "https://example.invalid/search?user=alice&nonce=0", new { value = 13 }, numericQueryParameter: "nonce");
        using HttpRequestMessage request = new(HttpMethod.Get, uri);

        var send = async () =>
        {
            using var response = await api.SendAsync(request, TestContext.Current.CancellationToken);
        };

        await send.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task NumericQueryAllowance_AcceptsChangingSdkNonce()
    {
        ExternalApi api = new();
        api.Json("GET", "https://example.invalid/search?user=alice&nonce=0", new { value = 13 }, numericQueryParameter: "nonce");
        using HttpRequestMessage request = new(HttpMethod.Get, "https://example.invalid/search?nonce=123&user=alice");

        using var response = await api.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        api.EnsureExpectationsMet();
    }
}
