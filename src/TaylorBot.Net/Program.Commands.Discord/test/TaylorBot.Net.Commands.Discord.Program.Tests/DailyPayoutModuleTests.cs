using Discord;
using FakeItEasy;
using FluentAssertions;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.DailyPayout.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests
{
    public class DailyPayoutModuleTests
    {
        private readonly IUser _commandUser = A.Fake<IUser>();
        private readonly ITaylorBotCommandContext _commandContext = A.Fake<ITaylorBotCommandContext>(o => o.Strict());
        private readonly IDailyPayoutRepository _dailyPayoutRepository = A.Fake<IDailyPayoutRepository>(o => o.Strict());
        private readonly IMessageOfTheDayRepository _messageOfTheDayRepository = A.Fake<IMessageOfTheDayRepository>(o => o.Strict());
        private readonly DailyPayoutModule _dailyPayoutModule;

        public DailyPayoutModuleTests()
        {
            _dailyPayoutModule = new DailyPayoutModule(_dailyPayoutRepository, _messageOfTheDayRepository);
            _dailyPayoutModule.SetContext(_commandContext);
            A.CallTo(() => _commandContext.User).Returns(_commandUser);
            A.CallTo(() => _commandContext.CommandPrefix).Returns("!");
        }

        [Theory]
        [InlineData(5, 13, 15)]
        [InlineData(5, 15, 20)]
        [InlineData(2, 1, 2)]
        public async Task DailyAsync_ThenReturnsEmbedWithNextBonus(uint daysForBonus, uint currentStreak, uint streakForNextBonus)
        {
            A.CallTo(() => _messageOfTheDayRepository.GetAllMessagesAsync()).Returns(new[] { new MessageOfTheDay("Hello", null) });
            A.CallTo(() => _dailyPayoutRepository.CanUserRedeemAsync(_commandUser)).Returns(new UserCanRedeem());
            A.CallTo(() => _dailyPayoutRepository.RedeemDailyPayoutAsync(_commandUser)).Returns(new RedeemResult(
                payoutAmount: 0,
                bonusAmount: 0,
                totalTaypointCount: 0,
                currentDailyStreak: currentStreak,
                daysForBonus: daysForBonus
            ));

            var result = (TaylorBotEmbedResult)await _dailyPayoutModule.DailyAsync();

            result.Embed.Description.Should().MatchRegex(@$".*({currentStreak})\S*\/\S*({streakForNextBonus}).*");
        }
    }
}
