using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Discord;

public sealed class DiscordApiTests
{
    [Fact]
    public async Task SlashCommandDriver_UsesTheRequestedCommandsRegistration()
    {
        DiscordApi api = new();
        JsonElement dispatched = default;
        DiscordDriver driver = new(api, (_, payload) =>
        {
            dispatched = JsonSerializer.SerializeToElement(payload);
            return Task.CompletedTask;
        });

        await driver.InvokeSlashCommandAsync(new("100000000000000003", "Alice"), "avatar");

        dispatched.GetProperty("data").GetProperty("id").GetString().Should()
            .Be(api.GetCommandId("avatar"))
            .And.NotBe(api.GetCommandId("taypoints"));
    }

    [Fact]
    public void UnmatchedRequest_RemainsAFailureEvenWhenCallerCatchesIt()
    {
        DiscordApi api = new();

        var request = () => api.Send("POST", "webhooks/unregistered/token", "{}");
        var verify = api.EnsureNoUnexpectedRequests;

        request.Should().Throw<InvalidOperationException>().WithMessage("*Unexpected Discord request*");
        verify.Should().Throw<InvalidOperationException>().WithMessage("*Unexpected Discord request*");
    }

    [Fact]
    public async Task UnconfiguredExternalHost_NeverUsesNetworkTransport()
    {
        DiscordApi api = new();
        using DiscordHttpHandler handler = new(api);
        using HttpClient client = new(handler, disposeHandler: false);

        var request = async () =>
        {
            using var response = await client.GetAsync("https://example.invalid/unconfigured", TestContext.Current.CancellationToken);
        };

        await request.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Unconfigured external request*");
        var verify = api.EnsureNoUnexpectedRequests;
        verify.Should().Throw<InvalidOperationException>();
    }
}
