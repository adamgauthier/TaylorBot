using Discord;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules;
using TaylorBot.Net.Commands.Discord.Program.Options;
using TaylorBot.Net.Commands.Discord.Program.Taypoints.Domain;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Snowflake;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests
{
    public class TaypointWillModuleTests
    {
        private readonly ITaylorBotCommandContext _commandContext = A.Fake<ITaylorBotCommandContext>(o => o.Strict());
        private readonly IOptionsMonitor<TaypointWillOptions> _options = A.Fake<IOptionsMonitor<TaypointWillOptions>>(o => o.Strict());
        private readonly ITaypointWillRepository _taypointWillRepository = A.Fake<ITaypointWillRepository>(o => o.Strict());
        private readonly TaypointWillModule _taypointWillModule;

        public TaypointWillModuleTests()
        {
            _taypointWillModule = new TaypointWillModule(_options, _taypointWillRepository);
            _taypointWillModule.SetContext(_commandContext);
        }

        [Fact]
        public async Task ClaimAsync_WhenWillWith1DayAfterInactivityThreshold_ThenReturnsErrorEmbed()
        {
            const uint InactiveDaysForClaim = 20;
            A.CallTo(() => _options.CurrentValue).Returns(new TaypointWillOptions { DaysOfInactivityBeforeWillCanBeClaimed = InactiveDaysForClaim });
            var oneDayAfterThreshold = DateTimeOffset.Now.AddDays((-InactiveDaysForClaim) + 1);
            var user = A.Fake<IUser>();
            A.CallTo(() => _commandContext.User).Returns(user);
            A.CallTo(() => _taypointWillRepository.GetWillsWithBeneficiaryAsync(user)).Returns(new[] { new Will(
                ownerUserId: new SnowflakeId("1"), ownerLatestSpokeAt: oneDayAfterThreshold
            )});

            var result = (TaylorBotEmbedResult)await _taypointWillModule.ClaimAsync();

            result.Embed.Color.Should().Be(TaylorBotColors.ErrorColor);
        }
    }
}
