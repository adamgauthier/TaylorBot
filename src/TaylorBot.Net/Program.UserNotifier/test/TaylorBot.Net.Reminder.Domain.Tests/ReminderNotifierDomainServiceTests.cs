using Discord;
using FluentAssertions;
using Xunit;

namespace TaylorBot.Net.Reminder.Domain.Tests;

public class ReminderNotifierDomainServiceTests
{
    [Theory]
    [InlineData((int)DiscordErrorCode.CannotSendMessageToUser)]
    [InlineData(ReminderNotifierDomainService.CannotSendMessagesToUserDueToNoMutualGuildsDiscordCode)]
    public void IsUndeliverableReminder_WhenDiscordRejectsDmPermanently_ThenReturnsTrue(int discordErrorCode)
    {
        var isUndeliverable = ReminderNotifierDomainService.IsUndeliverableReminder((DiscordErrorCode)discordErrorCode);

        isUndeliverable.Should().BeTrue();
    }

    [Fact]
    public void IsUndeliverableReminder_WhenDiscordErrorIsNull_ThenReturnsFalse()
    {
        var isUndeliverable = ReminderNotifierDomainService.IsUndeliverableReminder(null);

        isUndeliverable.Should().BeFalse();
    }

    [Fact]
    public void IsUndeliverableReminder_WhenDiscordErrorIsUnhandled_ThenReturnsFalse()
    {
        var isUndeliverable = ReminderNotifierDomainService.IsUndeliverableReminder(DiscordErrorCode.UnknownUser);

        isUndeliverable.Should().BeFalse();
    }
}
