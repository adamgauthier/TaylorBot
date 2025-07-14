using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Commands.Discord.Program.Modules.DailyPayout.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.DailyPayout.Domain;
using TaylorBot.Net.Commands.Discord.Program.Options;
using TaylorBot.Net.Commands.Discord.Program.Tests.Helpers;
using TaylorBot.Net.Core.User;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests;

public class DailyClaimCommandTests
{
    private readonly DiscordUser _commandUser = CommandUtils.AUser;
    private readonly IOptionsMonitor<DailyPayoutOptions> _options = A.Fake<IOptionsMonitor<DailyPayoutOptions>>(o => o.Strict());
    private readonly IDailyPayoutRepository _dailyPayoutRepository = A.Fake<IDailyPayoutRepository>(o => o.Strict());
    private readonly IMessageOfTheDayRepository _messageOfTheDayRepository = A.Fake<IMessageOfTheDayRepository>(o => o.Strict());
    private readonly DailyClaimSlashCommand _dailyClaimCommand;

    public DailyClaimCommandTests()
    {
        A.CallTo(() => _options.CurrentValue).Returns(new() { LegacyDailyPayoutAmount = 0 });
        _dailyClaimCommand = new(_options, _dailyPayoutRepository, _messageOfTheDayRepository, TimeProvider.System, CommandUtils.Mentioner);
    }

    [Theory]
    [InlineData(5, 13, 15)]
    [InlineData(5, 15, 20)]
    [InlineData(2, 1, 2)]
    public async Task Claim_ThenReturnsEmbedWithNextBonus(uint daysForBonus, uint currentStreak, uint streakForNextBonus)
    {
        A.CallTo(() => _messageOfTheDayRepository.GetAllMessagesAsync()).Returns([new MessageOfTheDay(Guid.NewGuid(), "Hello", null)]);
        A.CallTo(() => _dailyPayoutRepository.CanUserRedeemAsync(_commandUser)).Returns(new UserCanRedeem());
        A.CallTo(() => _dailyPayoutRepository.RedeemDailyPayoutAsync(_commandUser, 0)).Returns(new RedeemResult(
            BonusAmount: 0,
            TotalTaypointCount: 0,
            CurrentDailyStreak: currentStreak,
            DaysForBonus: daysForBonus
        ));

        var result = (EmbedResult)await _dailyClaimCommand.Claim(_commandUser, CommandUtils.CreateTestContext(_dailyClaimCommand)).RunAsync();

        result.Embed.Description.Should().MatchRegex(@$".*({currentStreak})\S*\/\S*({streakForNextBonus}).*");
    }
}
