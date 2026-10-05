using FluentAssertions;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Discord;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Commands.Events;

[Trait("Command", "coupon redeem")]
[Trait("Command", "coupon show")]
[Trait("Command", "owner addcoupon")]
[Trait("Command", "owner showcoupons")]
public sealed class CouponTests(DataServices data)
{
    [Fact]
    public async Task GuildMention_UsesInteractionIdWithoutLookup()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.EventGuildAsync(user);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "coupon show", guild);

        response.ShouldBeSuccess();
        response.Description.Should().Contain($"</coupon redeem:{scenario.DiscordApi.GetCommandId("coupon", guild.Id)}>");
        scenario.DiscordApi.RequestsFor("GET", $"applications/{DiscordApi.ApplicationId}/guilds/{guild.Id}/commands").Should().BeEmpty();
    }

    [Fact]
    public async Task Redeem_RecordsRewardAndHistoryOnlyOnce()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync(taypoints: 10);
        var guild = await scenario.Given.EventGuildAsync(user);
        await scenario.Given.CouponAsync();

        var redeemed = await scenario.Discord.InvokeSlashCommandAsync(user, "coupon redeem", guild, arguments: [SlashArgument.Text("code", "ENCHANTED")]);
        var repeated = await scenario.Discord.InvokeSlashCommandAsync(user, "coupon redeem", guild, arguments: [SlashArgument.Text("code", "ENCHANTED")]);
        var history = await scenario.Discord.InvokeSlashCommandAsync(user, "coupon show", guild);

        redeemed.ShouldBeSuccess();
        redeemed.Description.Should().Contain("50").And.Contain("60");
        repeated.ShouldBeError();
        repeated.Description.Should().Contain("already redeemed");
        history.Description.Should().Contain("ENCHANTED").And.Contain("50");
        (await scenario.State.BalanceAsync(user)).Should().Be(60);
        (await scenario.State.CouponUsesAsync()).Should().Be(1);
        (await scenario.State.RedeemedCouponCountAsync(user)).Should().Be(1);
    }

    [Theory]
    [InlineData(-1, 0, "expired")]
    [InlineData(1, 2, "maximum amount")]
    public async Task Redeem_UnavailableCouponDoesNotChangeBalance(int validDays, int used, string expected)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync(taypoints: 10);
        var guild = await scenario.Given.EventGuildAsync(user);
        await scenario.Given.CouponAsync(validDays: validDays, used: used);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "coupon redeem", guild, arguments: [SlashArgument.Text("code", "ENCHANTED")]);

        response.ShouldBeError();
        response.Description.Should().Contain(expected);
        (await scenario.State.BalanceAsync(user)).Should().Be(10);
        (await scenario.State.RedeemedCouponCountAsync(user)).Should().Be(0);
    }

    [Fact]
    public async Task Redeem_UnknownCodeAndEmptyHistoryExplainNextSteps()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.EventGuildAsync(user);

        var invalid = await scenario.Discord.InvokeSlashCommandAsync(user, "coupon redeem", guild, arguments: [SlashArgument.Text("code", "UNKNOWN")]);
        var history = await scenario.Discord.InvokeSlashCommandAsync(user, "coupon show", guild);

        invalid.ShouldBeError();
        invalid.Description.Should().Contain("not valid");
        history.Description.Should().Contain("never redeemed").And.Contain("coupon redeem");
    }

    [Fact]
    public async Task Owner_CreatesCouponAndDisplaysItsRewardAndUsage()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var owner = await scenario.Given.UserAsync(botOwner: true);

        var added = await scenario.Discord.InvokeSlashCommandAsync(owner, "owner addcoupon", arguments:
        [
            SlashArgument.Text("code", "NEWCODE"),
            SlashArgument.Text("from", "1m"),
            SlashArgument.Text("until", "1d"),
            SlashArgument.Integer("limit", value: 3),
            SlashArgument.Integer("reward", value: 75),
        ]);
        var listed = await scenario.Discord.InvokeSlashCommandAsync(owner, "owner showcoupons", arguments: [SlashArgument.Text("codes", "NEWCODE")]);

        added.ShouldBeSuccess();
        (await scenario.State.CouponRewardAsync("NEWCODE")).Should().Be(75);
        (await scenario.State.CouponUsesAsync("NEWCODE")).Should().Be(0);
        listed.Description.Should().Contain("NEWCODE").And.Contain("75").And.Contain("0/3");
    }

    [Fact]
    public async Task AddCoupon_RejectsNonOwner()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "owner addcoupon", arguments:
        [
            SlashArgument.Text("code", "NEWCODE"),
            SlashArgument.Text("from", "1m"), SlashArgument.Text("until", "1d"),
            SlashArgument.Integer("limit", value: 3), SlashArgument.Integer("reward", value: 75),
        ]);

        response.ShouldBeError();
        response.Description.Should().Contain("owner");
        (await scenario.State.CouponRewardAsync("NEWCODE")).Should().Be(0);
    }

    [Fact]
    public async Task ShowCoupons_RejectsNonOwner()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "owner showcoupons",
            arguments: [SlashArgument.Text("codes", "NEWCODE")]);

        response.ShouldBeError();
        response.Description.Should().Contain("owner");
    }
}
