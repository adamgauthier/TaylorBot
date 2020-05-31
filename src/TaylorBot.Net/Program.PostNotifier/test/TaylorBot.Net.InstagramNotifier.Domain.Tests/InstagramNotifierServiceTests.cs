using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.InstagramNotifier.Domain.DiscordEmbed;
using TaylorBot.Net.InstagramNotifier.Domain.Options;
using Xunit;

namespace TaylorBot.Net.InstagramNotifier.Domain.Tests
{
    public class InstagramNotifierServiceTests
    {
        private readonly ILogger<InstagramNotifierService> _logger = A.Fake<ILogger<InstagramNotifierService>>();
        private readonly IOptionsMonitor<InstagramNotifierOptions> _options = A.Fake<IOptionsMonitor<InstagramNotifierOptions>>(o => o.Strict());
        private readonly IInstagramCheckerRepository _instagramCheckerRepository = A.Fake<IInstagramCheckerRepository>(o => o.Strict());
        private readonly IInstagramClient _instagramClient = A.Fake<IInstagramClient>(o => o.Strict());
        private readonly ITaylorBotClient _taylorBotClient = A.Fake<ITaylorBotClient>(o => o.Strict());

        private readonly InstagramNotifierService _instagramNotifierService;

        public InstagramNotifierServiceTests()
        {
            _instagramNotifierService = new InstagramNotifierService(
                _logger, _options, _instagramCheckerRepository, _instagramClient, new InstagramPostToEmbedMapper(_options), _taylorBotClient
            );
        }

        [Fact]
        public async Task CheckAllInstagramsAsync_WhenCantResolveGuild_ThenErrorIsLogged()
        {
            var guildId = new SnowflakeId(1);

            var instagramChecker = new InstagramChecker(
                guildId,
                channelId: new SnowflakeId(2),
                instagramUsername: "taylorswift",
                lastPostCode: "A_CODE",
                lastPostTakenAt: DateTimeOffset.UtcNow
            );

            A.CallTo(() => _options.CurrentValue).Returns(new InstagramNotifierOptions { TimeSpanBetweenRequests = TimeSpan.FromMilliseconds(1) });
            A.CallTo(() => _instagramCheckerRepository.GetInstagramCheckersAsync()).Returns(new[] { instagramChecker });
            var exception = new ArgumentException();
            A.CallTo(() => _taylorBotClient.ResolveRequiredGuild(guildId)).Throws(exception);


            await _instagramNotifierService.CheckAllInstagramsAsync();


            A.CallTo(_logger).Where(call =>
                call.Method.Name == nameof(ILogger.Log) &&
                call.GetArgument<LogLevel>(0) == LogLevel.Error &&
                call.GetArgument<Exception>(3) == exception
            ).MustHaveHappenedOnceExactly();
        }
    }
}
