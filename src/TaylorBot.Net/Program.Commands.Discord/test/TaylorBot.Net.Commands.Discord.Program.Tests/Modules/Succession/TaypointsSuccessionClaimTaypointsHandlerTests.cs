using FakeItEasy;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Commands.Discord.Program.Modules.TaypointWills.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.TaypointWills.Domain;
using TaylorBot.Net.Commands.Discord.Program.Options;
using TaylorBot.Net.Commands.Discord.Program.Tests.Helpers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.Core.User;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests.Modules.Succession;

public class TaypointsSuccessionClaimTaypointsHandlerTests
{
    private const string AUsername = "TaylorSwift13";
    private readonly ITaypointWillRepository _taypointWillRepository = A.Fake<ITaypointWillRepository>(o => o.Strict());
    private readonly IOptionsMonitor<TaypointWillOptions> _options = A.Fake<IOptionsMonitor<TaypointWillOptions>>(o => o.Strict());
    private readonly IInteractionResponseClient _responseClient = A.Fake<IInteractionResponseClient>();
    private readonly RunContext _runContext;
    private readonly TaypointsSuccessionClaimTaypointsHandler _handler;

    public TaypointsSuccessionClaimTaypointsHandlerTests()
    {
        _handler = new(
            TimeProvider.System,
            CommandUtils.Mentioner,
            _taypointWillRepository,
            _options,
            _responseClient
        );
        _runContext = CommandUtils.CreateTestContext();
    }

    [Fact]
    public async Task HandleAsync_WhenHasWillsToClaim_ThenTransfersPointsAndReturnsSuccessEmbed()
    {
        const uint InactiveDaysForClaim = 20;
        A.CallTo(() => _options.CurrentValue).Returns(new TaypointWillOptions { DaysOfInactivityBeforeWillCanBeClaimed = InactiveDaysForClaim });
        var oneDayBeforeThreshold = DateTimeOffset.UtcNow.AddDays(-(InactiveDaysForClaim + 1));
        var willOwnerId = new SnowflakeId("1");
        A.CallTo(() => _taypointWillRepository.GetWillsWithBeneficiaryAsync(_runContext.User)).Returns([
            new WillOwner(
                OwnerUserId: willOwnerId,
                OwnerUsername: AUsername,
                OwnerLatestSpokeAt: oneDayBeforeThreshold
            )
        ]);

        var ownerUserIds = new[] { willOwnerId };
        A.CallTo(() => _taypointWillRepository.TransferAllPointsAsync(
            A<IReadOnlyCollection<SnowflakeId>>.That.IsSameSequenceAs(ownerUserIds),
            _runContext.User
        )).Returns([
            new Transfer(willOwnerId, AUsername, TaypointCount: 0, OriginalTaypointCount: 100),
            new Transfer(_runContext.User.Id, AUsername, TaypointCount: 100, OriginalTaypointCount: 0)
        ]);

        A.CallTo(() => _taypointWillRepository.RemoveWillsWithBeneficiaryAsync(
            A<IReadOnlyCollection<SnowflakeId>>.That.IsSameSequenceAs(ownerUserIds),
            _runContext.User
        )).Returns(default);

        await _handler.HandleAsync(A.Fake<DiscordButtonComponent>(), _runContext);

        A.CallTo(() => _taypointWillRepository.TransferAllPointsAsync(
            A<IReadOnlyCollection<SnowflakeId>>.That.IsSameSequenceAs(ownerUserIds),
            _runContext.User
        )).MustHaveHappenedOnceExactly();

        A.CallTo(() => _taypointWillRepository.RemoveWillsWithBeneficiaryAsync(
            A<IReadOnlyCollection<SnowflakeId>>.That.IsSameSequenceAs(ownerUserIds),
            _runContext.User
        )).MustHaveHappenedOnceExactly();

        A.CallTo(() => _responseClient.EditOriginalResponseAsync(
            A<ParsedInteraction>._,
            A<DiscordEmbed>.That.Matches(e =>
                e.color == TaylorBotColors.SuccessColor
            )
        )).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task HandleAsync_WhenNoWillsToClaim_ThenReturnsErrorEmbed()
    {
        const uint InactiveDaysForClaim = 20;
        A.CallTo(() => _options.CurrentValue).Returns(new TaypointWillOptions { DaysOfInactivityBeforeWillCanBeClaimed = InactiveDaysForClaim });
        var oneDayAfterThreshold = DateTimeOffset.UtcNow.AddDays(-InactiveDaysForClaim + 1);
        A.CallTo(() => _taypointWillRepository.GetWillsWithBeneficiaryAsync(_runContext.User)).Returns([
            new WillOwner(
                OwnerUserId: new("1"),
                OwnerUsername: AUsername,
                OwnerLatestSpokeAt: oneDayAfterThreshold
            )
        ]);

        await _handler.HandleAsync(A.Fake<DiscordButtonComponent>(), _runContext);

        A.CallTo(() => _taypointWillRepository.TransferAllPointsAsync(
            A<IReadOnlyCollection<SnowflakeId>>._,
            A<DiscordUser>._
        )).MustNotHaveHappened();

        A.CallTo(() => _responseClient.EditOriginalResponseAsync(
            A<ParsedInteraction>._,
            A<DiscordEmbed>.That.Matches(e =>
                e.color == TaylorBotColors.ErrorColor
            )
        )).MustHaveHappenedOnceExactly();
    }
}
