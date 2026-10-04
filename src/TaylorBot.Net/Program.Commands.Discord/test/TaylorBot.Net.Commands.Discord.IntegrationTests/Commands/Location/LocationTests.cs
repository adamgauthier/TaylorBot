using FluentAssertions;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Discord;
using TaylorBot.Net.Commands.Discord.IntegrationTests.ExternalApis;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Scenarios;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Commands.Location;

[Trait("Command", "location show")]
[Trait("Command", "location set")]
[Trait("Command", "location time")]
[Trait("Command", "location weather")]
[Trait("Command", "location clear")]
public sealed class LocationTests(DataServices data)
{
    [Fact]
    public async Task Set_PersistsGeneralLocationAndResolvedTimezone()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        scenario.External.Place();
        scenario.External.PlaceTimeZone();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "location set",
            arguments: [SlashArgument.Text("location", "Quebec City")]);

        response.ShouldBeSuccess();
        response.Description.Should().Contain("Quebec City, QC, Canada");
        (await scenario.State.LocationAsync(user)).Should().Be(new LocationState("Quebec City, QC, Canada", "46.8130816", "-71.2074596", "America/Toronto"));
        scenario.External.Requests.Should().Contain(request => request.Method == "POST" && request.Body!.Contains("Quebec City"));
    }

    [Fact]
    public async Task Set_PreciseStreetAddressIsRejectedWithoutOverwritingLocation()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        await scenario.Given.LocationAsync(user);
        scenario.External.Place("street_address");

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "location set",
            arguments: [SlashArgument.Text("location", "123 Main Street")]);

        response.ShouldBeError();
        response.Description.Should().Contain("too specific");
        (await scenario.State.LocationAsync(user))!.Address.Should().Be("Quebec City, QC, Canada");
    }

    [Fact]
    public async Task Set_UnknownPlaceDoesNotPersist()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        scenario.External.PlaceNotFound();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "location set",
            arguments: [SlashArgument.Text("location", "Imaginary city")]);

        response.ShouldBeError();
        response.Description.Should().Contain("Unable to find");
        (await scenario.State.LocationAsync(user)).Should().BeNull();
    }

    [Fact]
    public async Task Set_PlaceServiceFailureDoesNotPersist()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        scenario.External.PlaceUnavailable();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "location set",
            arguments: [SlashArgument.Text("location", "Quebec City")]);

        response.ShouldBeError();
        response.Description.Should().Contain("location service might be down");
        (await scenario.State.LocationAsync(user)).Should().BeNull();
    }

    [Fact]
    public async Task Set_TimezoneFailureDoesNotPersist()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        scenario.External.Place();
        scenario.External.PlaceTimeZone("ZERO_RESULTS");

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "location set",
            arguments: [SlashArgument.Text("location", "Quebec City")]);

        response.ShouldBeError();
        response.Description.Should().Contain("location service might be down");
        (await scenario.State.LocationAsync(user)).Should().BeNull();
    }

    [Theory]
    [InlineData("location show")]
    [InlineData("location time")]
    public async Task ShowAndTime_DisplaySelectedUsersLocation(string command)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var other = await scenario.Given.UserAsync(username: "Other");
        await scenario.Given.LocationAsync(other);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, command, selectedUser: other);

        response.ShouldBeSuccess();
        response.Description.Should().Contain("Other").And.Contain("Quebec City, QC, Canada").And.Contain("Eastern Standard Time");
    }

    [Theory]
    [InlineData("location show")]
    [InlineData("location time")]
    [InlineData("location weather")]
    public async Task ShowTimeAndWeather_UnsetExplainHowToSet(string command)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, command);

        response.ShouldBeError();
        response.Description.Should().Contain("not set").And.Contain("location set");
    }

    [Fact]
    public async Task Clear_RemovesOnlyCallersLocation()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var other = await scenario.Given.UserAsync();
        await scenario.Given.LocationAsync(user);
        await scenario.Given.LocationAsync(other);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "location clear");

        response.ShouldBeSuccess();
        (await scenario.State.LocationAsync(user)).Should().BeNull();
        (await scenario.State.LocationAsync(other)).Should().NotBeNull();
    }

    [Fact]
    public async Task LegacyLocation_DisplaysStoredLocation()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.LocationAsync(user);

        var response = await scenario.Discord.SendMessageAsync(user, guild, "!location");

        response.ShouldBeSuccess();
        response.Description.Should().Contain("Quebec City, QC, Canada");
    }

    [Fact]
    public async Task LegacyWeather_DisplaysStoredLocationsForecast()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.LocationAsync(user);
        scenario.External.Weather();

        var response = await scenario.Discord.SendMessageAsync(user, guild, "!weather");

        response.ShouldBeSuccess();
        response.Description.Should().Contain("13.3\u00b0C");
        response.Embed.GetProperty("footer").GetProperty("text").GetString().Should().Be("Quebec City, QC, Canada");
    }

    [Fact]
    public async Task Weather_LocationOverrideDoesNotSetProfileLocation()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        scenario.External.Place();
        scenario.External.Weather();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "location weather",
            arguments: [SlashArgument.Text("location", "Quebec City")]);

        response.ShouldBeSuccess();
        response.Description.Should().Contain("13.3\u00b0C");
        response.Embed.GetProperty("footer").GetProperty("text").GetString().Should().Be("Quebec City, QC, Canada");
        (await scenario.State.LocationAsync(user)).Should().BeNull();
    }

    [Fact]
    public async Task Weather_ProviderFailureExplainsRetry()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        await scenario.Given.LocationAsync(user);
        scenario.External.ForecastUnavailable();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "location weather");

        response.ShouldBeError();
        response.Description.Should().Contain("weather service might be down");
        (await scenario.State.LocationAsync(user)).Should().NotBeNull();
    }
}
