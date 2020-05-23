using Discord;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules;
using TaylorBot.Net.Commands.Discord.Program.Options;
using TaylorBot.Net.Commands.Discord.Program.Taypoints.Domain;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Snowflake;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests
{
    public class TaypointWillModuleTests
    {
        private readonly ITaylorBotCommandContext _commandContext = A.Fake<ITaylorBotCommandContext>(o => o.Strict());
        private readonly IUser _commandUser = A.Fake<IUser>();
        private readonly IOptionsMonitor<TaypointWillOptions> _options = A.Fake<IOptionsMonitor<TaypointWillOptions>>(o => o.Strict());
        private readonly ITaypointWillRepository _taypointWillRepository = A.Fake<ITaypointWillRepository>(o => o.Strict());
        private readonly TaypointWillModule _taypointWillModule;

        public TaypointWillModuleTests()
        {
            A.CallTo(() => _commandContext.User).Returns(_commandUser);
            _taypointWillModule = new TaypointWillModule(_options, _taypointWillRepository);
            _taypointWillModule.SetContext(_commandContext);
        }

        [Fact]
        public async Task AddAsync_WhenWillNotAdded_ThenReturnsErrorEmbed()
        {
            A.CallTo(() => _commandContext.CommandPrefix).Returns(string.Empty);
            var user = A.Fake<IUser>();
            A.CallTo(() => _taypointWillRepository.AddWillAsync(_commandUser, user)).Returns(new WillNotAddedResult(new SnowflakeId("1")));
            var userArgument = A.Fake<IMentionedUserNotAuthor<IUser>>();
            A.CallTo(() => userArgument.GetTrackedUserAsync()).Returns(user);

            var result = (TaylorBotEmbedResult)await _taypointWillModule.AddAsync(userArgument);

            result.Embed.Color.Should().Be(TaylorBotColors.ErrorColor);
        }

        [Fact]
        public async Task AddAsync_WhenWillAdded_ThenReturnsSuccessEmbed()
        {
            A.CallTo(() => _commandContext.CommandPrefix).Returns(string.Empty);
            A.CallTo(() => _options.CurrentValue).Returns(new TaypointWillOptions { DaysOfInactivityBeforeWillCanBeClaimed = 10 });
            var user = A.Fake<IUser>();
            A.CallTo(() => _taypointWillRepository.AddWillAsync(_commandUser, user)).Returns(new WillAddedResult());
            var userArgument = A.Fake<IMentionedUserNotAuthor<IUser>>();
            A.CallTo(() => userArgument.GetTrackedUserAsync()).Returns(user);

            var result = (TaylorBotEmbedResult)await _taypointWillModule.AddAsync(userArgument);

            result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
        }

        [Fact]
        public async Task ClaimAsync_WhenWillWith1DayAfterInactivityThreshold_ThenReturnsErrorEmbed()
        {
            const uint InactiveDaysForClaim = 20;
            A.CallTo(() => _options.CurrentValue).Returns(new TaypointWillOptions { DaysOfInactivityBeforeWillCanBeClaimed = InactiveDaysForClaim });
            var oneDayAfterThreshold = DateTimeOffset.Now.AddDays((-InactiveDaysForClaim) + 1);
            A.CallTo(() => _taypointWillRepository.GetWillsWithBeneficiaryAsync(_commandUser)).Returns(new[] { new Will(
                ownerUserId: new SnowflakeId("1"), ownerLatestSpokeAt: oneDayAfterThreshold
            )});

            var result = (TaylorBotEmbedResult)await _taypointWillModule.ClaimAsync();

            result.Embed.Color.Should().Be(TaylorBotColors.ErrorColor);
        }

        [Fact]
        public async Task ClaimAsync_WhenWillWithInactiveOwner_ThenReturnsSuccessEmbed()
        {
            var willOwnerId = new SnowflakeId(1);
            var commandUserId = new SnowflakeId(2);
            A.CallTo(() => _commandUser.Id).Returns(commandUserId.Id);
            const uint InactiveDaysForClaim = 20;
            A.CallTo(() => _options.CurrentValue).Returns(new TaypointWillOptions { DaysOfInactivityBeforeWillCanBeClaimed = InactiveDaysForClaim });
            var oneDayBeforeThreshold = DateTimeOffset.Now.AddDays(-(InactiveDaysForClaim + 1));
            A.CallTo(() => _taypointWillRepository.GetWillsWithBeneficiaryAsync(_commandUser)).Returns(new[] { new Will(
                ownerUserId: willOwnerId, ownerLatestSpokeAt: oneDayBeforeThreshold
            )});
            var ownerUserIds = new[] { willOwnerId };
            A.CallTo(() => _taypointWillRepository.TransferAllPointsAsync(A<IReadOnlyCollection<SnowflakeId>>.That.IsSameSequenceAs(ownerUserIds), _commandUser)).Returns(new[] {
                new Transfer(willOwnerId, 0, 100),
                new Transfer(commandUserId, 100, 0)
            });
            A.CallTo(() => _taypointWillRepository.RemoveWillsAsync(A<IReadOnlyCollection<SnowflakeId>>.That.IsSameSequenceAs(ownerUserIds), _commandUser)).Returns(default);

            var result = (TaylorBotEmbedResult)await _taypointWillModule.ClaimAsync();

            result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
        }
    }
}
