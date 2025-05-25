using FakeItEasy;
using TaylorBot.Net.Commands.Discord.Program.Modules.TaypointWills.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.TaypointWills.Domain;
using TaylorBot.Net.Commands.Discord.Program.Tests.Helpers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.User;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests.Modules.Succession;

public class TaypointsSuccessionManageSuccessorHandlerTests
{
    private const string AUsername = "TaylorSwift13";
    private readonly ITaypointWillRepository _taypointWillRepository = A.Fake<ITaypointWillRepository>(o => o.Strict());
    private readonly IInteractionResponseClient _responseClient = A.Fake<IInteractionResponseClient>();
    private readonly RunContext _runContext;
    private readonly TaypointsSuccessionClearSuccessorHandler _clearHandler;
    private readonly TaypointsSuccessionChangeSuccessorHandler _changeHandler;

    public TaypointsSuccessionManageSuccessorHandlerTests()
    {
        _clearHandler = new(_taypointWillRepository, _responseClient);
        _changeHandler = new(_taypointWillRepository, _responseClient);
        _runContext = CommandUtils.CreateTestContext();
    }

    [Fact]
    public async Task ClearSuccessor_WhenWillRemoved_ThenReturnsSuccessEmbed()
    {
        A.CallTo(() => _taypointWillRepository.RemoveWillWithOwnerAsync(_runContext.User)).Returns(new WillRemovedResult(new("1"), AUsername));

        await _clearHandler.HandleAsync(A.Fake<DiscordButtonComponent>(), _runContext);

        A.CallTo(() => _taypointWillRepository.RemoveWillWithOwnerAsync(_runContext.User)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _responseClient.EditOriginalResponseAsync(
            A<ParsedInteraction>._,
            A<MessageResponse>.That.Matches(r =>
                r.Content.Embeds.Single().Color == TaylorBotColors.SuccessColor
            )
        )).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ChangeSuccessor_WhenSelfSelected_ThenReturnsErrorEmbed()
    {
        DiscordUserSelectComponent userSelect = new(null!, null!, null!, [_runContext.User]);

        await _changeHandler.HandleAsync(userSelect, _runContext);

        A.CallTo(() => _taypointWillRepository.AddWillAsync(
            A<DiscordUser>._,
            A<DiscordUser>._
        )).MustNotHaveHappened();
        A.CallTo(() => _responseClient.EditOriginalResponseAsync(
            A<ParsedInteraction>._,
            A<DiscordEmbed>.That.Matches(e =>
                e.color == TaylorBotColors.ErrorColor
            )
        )).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ChangeSuccessor_WhenOtherUserSelected_ThenAddsWillAndReturnsSuccessEmbed()
    {
        var selectedUser = _runContext.User with { Id = new("2") };
        DiscordUserSelectComponent userSelect = new(null!, null!, null!, [selectedUser]);

        A.CallTo(() => _taypointWillRepository.AddWillAsync(_runContext.User, selectedUser)).Returns(new());

        await _changeHandler.HandleAsync(userSelect, _runContext);

        A.CallTo(() => _taypointWillRepository.AddWillAsync(_runContext.User, selectedUser)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _responseClient.EditOriginalResponseAsync(
            A<ParsedInteraction>._,
            A<MessageResponse>.That.Matches(r =>
                r.Content.Embeds.Single().Color == TaylorBotColors.SuccessColor
            )
        )).MustHaveHappenedOnceExactly();
    }
}
