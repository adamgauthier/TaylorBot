using Discord;
using FluentAssertions;
using TaylorBot.Net.Core.Client;
using Xunit;

namespace TaylorBot.Net.Core.Tests.Client;

public class DiscordDmErrorTests
{
    [Theory]
    [InlineData((int)DiscordErrorCode.CannotSendMessageToUser)]
    [InlineData(DiscordDmError.CannotSendMessagesToUserDueToDmRestrictionsDiscordCode)]
    public void IsUndeliverable_WhenDiscordRejectsDm_ThenReturnsTrue(int discordErrorCode)
    {
        var isUndeliverable = DiscordDmError.IsUndeliverable((DiscordErrorCode)discordErrorCode);

        isUndeliverable.Should().BeTrue();
    }

    [Fact]
    public void IsUndeliverable_WhenDiscordErrorIsNull_ThenReturnsFalse()
    {
        var isUndeliverable = DiscordDmError.IsUndeliverable((DiscordErrorCode?)null);

        isUndeliverable.Should().BeFalse();
    }

    [Fact]
    public void IsUndeliverable_WhenDiscordErrorIsUnhandled_ThenReturnsFalse()
    {
        var isUndeliverable = DiscordDmError.IsUndeliverable(DiscordErrorCode.UnknownUser);

        isUndeliverable.Should().BeFalse();
    }
}
