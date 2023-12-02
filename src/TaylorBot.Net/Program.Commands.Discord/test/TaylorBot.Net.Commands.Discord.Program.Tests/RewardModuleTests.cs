using Discord;
using FakeItEasy;
using FluentAssertions;
using TaylorBot.Net.Commands.Discord.Program.Modules.TaypointReward.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.TaypointReward.Domain;
using TaylorBot.Net.Commands.Discord.Program.Tests.Helpers;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Snowflake;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests;

public class RewardModuleTests
{
    private readonly IUser _commandUser = A.Fake<IUser>();
    private readonly ITaylorBotCommandContext _commandContext = A.Fake<ITaylorBotCommandContext>();
    private readonly ITaypointRewardRepository _taypointRewardRepository = A.Fake<ITaypointRewardRepository>(o => o.Strict());
    private readonly RewardModule _rewardModule;

    public RewardModuleTests()
    {
        _rewardModule = new RewardModule(new SimpleCommandRunner(), _taypointRewardRepository);
        _rewardModule.SetContext(_commandContext);
        A.CallTo(() => _commandContext.User).Returns(_commandUser);
    }

    [Fact]
    public async Task RewardAsync_ThenReturnsEmbedWithUserIdAndNewTaypointCountOnOneLine()
    {
        const int RewardedTaypointCount = 13;
        const string UserId = "12345678901";
        const int NewTaypointCount = 22;
        var user = A.Fake<IUser>();
        var userArgument = A.Fake<IMentionedUserNotAuthor<IUser>>(o => o.Strict());
        A.CallTo(() => userArgument.GetTrackedUserAsync()).Returns(user);

        A.CallTo(() => _taypointRewardRepository.RewardUsersAsync(A<IReadOnlyCollection<IUser>>.That.IsSameSequenceAs(new[] { user }), RewardedTaypointCount))
            .Returns(new[] { new RewardedUserResult(new SnowflakeId(UserId), NewTaypointCount) });

        var result = (await _rewardModule.RewardAsync(new PositiveInt32(RewardedTaypointCount), new[] { userArgument })).GetResult<EmbedResult>();

        result.Embed.Description.Split('\n')
            .Should().ContainSingle(line => line.Contains(UserId) && line.Contains(NewTaypointCount.ToString()));
    }
}
