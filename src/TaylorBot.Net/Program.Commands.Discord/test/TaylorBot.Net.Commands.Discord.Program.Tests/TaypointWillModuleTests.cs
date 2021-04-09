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
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Snowflake;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests
{
    public class TaypointWillModuleTests
    {
        private const string AUsername = "TaylorSwift13";

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
            A.CallTo(() => _commandContext.CommandPrefix).Returns("!");
        }

        [Fact]
        public async Task GetAsync_WhenWillDoesntExist_ThenReturnsEmbedWithNoBeneficiary()
        {
            var user = A.Fake<IUser>();
            A.CallTo(() => _taypointWillRepository.GetWillAsync(user)).Returns(null);
            var userArgument = A.Fake<IUserArgument<IUser>>();
            A.CallTo(() => userArgument.GetTrackedUserAsync()).Returns(user);

            var result = (TaylorBotEmbedResult)await _taypointWillModule.GetAsync(userArgument);

            result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
            result.Embed.Description.Should().Contain("no beneficiary");
        }

        [Fact]
        public async Task GetAsync_WhenWillExists_ThenReturnsEmbedWithBeneficiaryMention()
        {
            const uint InactiveDaysForClaim = 20;
            A.CallTo(() => _options.CurrentValue).Returns(new TaypointWillOptions { DaysOfInactivityBeforeWillCanBeClaimed = InactiveDaysForClaim });
            var beneficiaryId = new SnowflakeId("1");
            var user = A.Fake<IUser>();
            A.CallTo(() => _taypointWillRepository.GetWillAsync(user)).Returns(new Will(beneficiaryId, AUsername));
            var userArgument = A.Fake<IUserArgument<IUser>>();
            A.CallTo(() => userArgument.GetTrackedUserAsync()).Returns(user);

            var result = (TaylorBotEmbedResult)await _taypointWillModule.GetAsync(userArgument);

            result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
            result.Embed.Description.Should().Contain(MentionUtils.MentionUser(beneficiaryId.Id));
        }

        [Fact]
        public async Task AddAsync_WhenWillNotAdded_ThenReturnsErrorEmbed()
        {
            var user = A.Fake<IUser>();
            A.CallTo(() => _taypointWillRepository.AddWillAsync(_commandUser, user)).Returns(new WillNotAddedResult(new SnowflakeId("1"), AUsername));
            var userArgument = A.Fake<IMentionedUserNotAuthor<IUser>>();
            A.CallTo(() => userArgument.GetTrackedUserAsync()).Returns(user);

            var result = (TaylorBotEmbedResult)await _taypointWillModule.AddAsync(userArgument);

            result.Embed.Color.Should().Be(TaylorBotColors.ErrorColor);
        }

        [Fact]
        public async Task AddAsync_WhenWillAdded_ThenReturnsSuccessEmbed()
        {
            A.CallTo(() => _options.CurrentValue).Returns(new TaypointWillOptions { DaysOfInactivityBeforeWillCanBeClaimed = 10 });
            var user = A.Fake<IUser>();
            A.CallTo(() => _taypointWillRepository.AddWillAsync(_commandUser, user)).Returns(new WillAddedResult());
            var userArgument = A.Fake<IMentionedUserNotAuthor<IUser>>();
            A.CallTo(() => userArgument.GetTrackedUserAsync()).Returns(user);

            var result = (TaylorBotEmbedResult)await _taypointWillModule.AddAsync(userArgument);

            result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
        }

        [Fact]
        public async Task ClearAsync_WhenWillNotRemoved_ThenReturnsErrorEmbed()
        {
            A.CallTo(() => _taypointWillRepository.RemoveWillWithOwnerAsync(_commandUser)).Returns(new WillNotRemovedResult());

            var result = (TaylorBotEmbedResult)await _taypointWillModule.ClearAsync();

            result.Embed.Color.Should().Be(TaylorBotColors.ErrorColor);
        }

        [Fact]
        public async Task ClearAsync_WhenWillRemoved_ThenReturnsSuccessEmbed()
        {
            A.CallTo(() => _taypointWillRepository.RemoveWillWithOwnerAsync(_commandUser)).Returns(new WillRemovedResult(new SnowflakeId(1), AUsername));

            var result = (TaylorBotEmbedResult)await _taypointWillModule.ClearAsync();

            result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
        }

        [Fact]
        public async Task ClaimAsync_WhenWillWith1DayAfterInactivityThreshold_ThenReturnsErrorEmbed()
        {
            const uint InactiveDaysForClaim = 20;
            A.CallTo(() => _options.CurrentValue).Returns(new TaypointWillOptions { DaysOfInactivityBeforeWillCanBeClaimed = InactiveDaysForClaim });
            var oneDayAfterThreshold = DateTimeOffset.Now.AddDays((-InactiveDaysForClaim) + 1);
            A.CallTo(() => _taypointWillRepository.GetWillsWithBeneficiaryAsync(_commandUser)).Returns(new[] { new WillOwner(
                OwnerUserId: new("1"), OwnerUsername: AUsername, OwnerLatestSpokeAt: oneDayAfterThreshold
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
            A.CallTo(() => _taypointWillRepository.GetWillsWithBeneficiaryAsync(_commandUser)).Returns(new[] { new WillOwner(
                OwnerUserId: willOwnerId, OwnerUsername: AUsername, OwnerLatestSpokeAt: oneDayBeforeThreshold
            )});
            var ownerUserIds = new[] { willOwnerId };
            A.CallTo(() => _taypointWillRepository.TransferAllPointsAsync(A<IReadOnlyCollection<SnowflakeId>>.That.IsSameSequenceAs(ownerUserIds), _commandUser)).Returns(new[] {
                new Transfer(willOwnerId, AUsername, TaypointCount: 0, OriginalTaypointCount: 100),
                new Transfer(commandUserId, AUsername, TaypointCount: 100, OriginalTaypointCount: 0)
            });
            A.CallTo(() => _taypointWillRepository.RemoveWillsWithBeneficiaryAsync(A<IReadOnlyCollection<SnowflakeId>>.That.IsSameSequenceAs(ownerUserIds), _commandUser)).Returns(default);

            var result = (TaylorBotEmbedResult)await _taypointWillModule.ClaimAsync();

            result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
        }
    }
}
