using FluentAssertions;
using System.Net;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Discord;
using TaylorBot.Net.Commands.Discord.IntegrationTests.ExternalApis;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Commands.Events;

[Trait("Command", "signature")]
[Trait("Command", "recap")]
public sealed class YearbookTests(DataServices data)
{
    [Fact]
    public async Task GuildMention_RefreshesIndependentlyOfGlobalMissCooldown()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.EventGuildAsync(user);
        scenario.DiscordApi.ExpectRequest("GET", $"applications/{DiscordApi.ApplicationId}/guilds/{guild.Id}/commands",
            Array.Empty<object>(), HttpStatusCode.OK);
        scenario.External.SignatureList(user, exists: false);
        var unavailable = await scenario.Discord.InvokeSlashCommandAsync(user, "recap", guild);
        await scenario.Given.DailyMessageAsync("Try `/hlep`.");
        await scenario.Discord.InvokeSlashCommandAsync(user, "daily claim", guild);
        using var gate = scenario.DiscordApi.PauseCommandLookups();

        var pendingResponse = scenario.Discord.InvokeSlashCommandAsync(user, "recap", guild);
        await gate.WaitForRequestAsync(TestContext.Current.CancellationToken);
        gate.Dispose();
        var response = await pendingResponse;

        response.ShouldBeError();
        unavailable.Description.Should().Contain("/signature").And.NotContain("</signature:");
        response.Description.Should().Contain($"</signature:{scenario.DiscordApi.GetCommandId("signature", guild.Id)}>");
        scenario.DiscordApi.RequestsFor("GET", $"applications/{DiscordApi.ApplicationId}/guilds/{guild.Id}/commands").Should().HaveCount(2);
        scenario.DiscordApi.RequestsFor("GET", $"applications/{DiscordApi.ApplicationId}/commands").Should().HaveCount(2);
    }

    [Fact]
    public async Task Signature_ConfirmationUploadsTheSubmittedImage()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.EventGuildAsync(user);
        await scenario.Given.JoinedBeforeAnniversaryAsync(guild, user);
        byte[] image = [1, 3, 3, 7];
        scenario.External.SignatureExists(user, exists: false);
        scenario.External.Bytes("https://example.invalid/signature.png", image);
        scenario.External.SignatureUpload(user);

        var prompt = await scenario.Discord.InvokeSlashCommandAsync(user, "signature", guild,
            arguments: [SlashArgument.Attachment("file", "https://example.invalid/signature.png")]);
        var confirmed = await scenario.Discord.ClickAsync(user, prompt, "Confirm");

        prompt.Embed.GetProperty("image").GetProperty("url").GetString().Should().Be("https://example.invalid/signature.png");
        confirmed.ShouldBeSuccess();
        confirmed.Description.Should().Contain("successfully uploaded");
        scenario.External.Requests.Should().ContainSingle(request => request.Method == "PUT").Which.Bytes.Should().Equal(image);
    }

    [Fact]
    public async Task Signature_ExistingSubmissionIsNotOverwritten()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.EventGuildAsync(user);
        await scenario.Given.JoinedBeforeAnniversaryAsync(guild, user);
        scenario.External.SignatureExists(user, exists: true);

        var prompt = await scenario.Discord.InvokeSlashCommandAsync(user, "signature", guild,
            arguments: [SlashArgument.Text("link", "https://example.invalid/signature.png")]);
        var confirmed = await scenario.Discord.ClickAsync(user, prompt, "Confirm");

        confirmed.ShouldBeError();
        confirmed.Description.Should().Contain("already uploaded");
        scenario.External.Requests.Should().ContainSingle().Which.Method.Should().Be("HEAD");
    }

    [Fact]
    public async Task Signature_RequiresExactlyOneImageSource()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.EventGuildAsync(user);
        await scenario.Given.JoinedBeforeAnniversaryAsync(guild, user);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "signature", guild);

        response.ShouldBeError();
        response.Description.Should().Contain("exactly one");
        scenario.External.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task Recap_AttachesPersistedImageAfterCheckingSignature()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.EventGuildAsync(user);
        byte[] image = [1, 3, 3, 7];
        await scenario.Given.RecapAsync(user, image);
        scenario.External.SignatureList(user, exists: true);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "recap", guild);

        response.ShouldBeSuccess();
        response.Embed.GetProperty("image").GetProperty("url").GetString().Should().Be("attachment://recap.png");
        response.Requests[1].Attachments.Should().ContainSingle().Which.Bytes.Should().Equal(image);
    }

    [Fact]
    public async Task Recap_MissingSignatureExplainsHowToSubmitOne()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.EventGuildAsync(user);
        scenario.External.SignatureList(user, exists: false);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "recap", guild);

        response.ShouldBeError();
        response.Description.Should().Contain("must submit").And.Contain("signature");
    }

    [Fact]
    public async Task Recap_MissingImageExplainsEligibility()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.EventGuildAsync(user);
        await scenario.Given.RecapAsync(user);
        scenario.External.SignatureList(user, exists: true);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "recap", guild);

        response.ShouldBeError();
        response.Description.Should().Contain("500").And.Contain("most active");
    }
}
