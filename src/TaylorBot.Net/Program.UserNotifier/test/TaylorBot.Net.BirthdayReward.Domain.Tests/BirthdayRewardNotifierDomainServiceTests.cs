using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaylorBot.Net.BirthdayReward.Domain.DiscordEmbed;
using TaylorBot.Net.BirthdayReward.Domain.Options;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Snowflake;
using Xunit;

namespace TaylorBot.Net.BirthdayReward.Domain.Tests
{
    public class BirthdayRewardNotifierDomainServiceTests
    {
        private readonly ILogger<BirthdayRewardNotifierDomainService> _logger = A.Fake<ILogger<BirthdayRewardNotifierDomainService>>();
        private readonly IOptionsMonitor<BirthdayRewardNotifierOptions> _options = A.Fake<IOptionsMonitor<BirthdayRewardNotifierOptions>>(o => o.Strict());
        private readonly IBirthdayRepository _birthdayRepository = A.Fake<IBirthdayRepository>(o => o.Strict());
        private readonly ITaylorBotClient _taylorBotClient = A.Fake<ITaylorBotClient>(o => o.Strict());

        private readonly BirthdayRewardNotifierDomainService _birthdayRewardNotifierDomainService;

        public BirthdayRewardNotifierDomainServiceTests()
        {
            _birthdayRewardNotifierDomainService = new BirthdayRewardNotifierDomainService(
                _logger, _options, _birthdayRepository, new BirthdayRewardEmbedFactory(), new(_taylorBotClient)
            );
        }

        [Fact]
        public async Task RewardBirthdaysAsync_WhenCantResolveUser_ThenErrorIsLogged()
        {
            const uint RewardAmount = 1989;
            var userId = new SnowflakeId(1);

            var rewardedUser = new RewardedUser(
                userId,
                rewardedPoints: RewardAmount
            );

            A.CallTo(() => _options.CurrentValue).Returns(new BirthdayRewardNotifierOptions { RewardAmount = RewardAmount });
            A.CallTo(() => _birthdayRepository.RewardEligibleUsersAsync(RewardAmount)).Returns(new[] { rewardedUser });
            var exception = new ArgumentException();
            A.CallTo(() => _taylorBotClient.ResolveRequiredUserAsync(userId)).Throws(exception);


            await _birthdayRewardNotifierDomainService.RewardBirthdaysAsync();


            A.CallTo(_logger).Where(call =>
                call.Method.Name == nameof(ILogger.Log) &&
                call.GetArgument<LogLevel>(0) == LogLevel.Error &&
                call.GetArgument<Exception>(3) == exception
            ).MustHaveHappenedOnceExactly();
        }
    }
}
