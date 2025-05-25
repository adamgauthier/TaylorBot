using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Commands.Discord.Program.Modules.TaypointWills.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.TaypointWills.Domain;
using TaylorBot.Net.Commands.Discord.Program.Options;
using TaylorBot.Net.Commands.Discord.Program.Tests.Helpers;
using TaylorBot.Net.Core.Colors;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests.Modules.Succession;

public class TaypointsSuccessionSlashCommandTests
{
    private const string AUsername = "TaylorSwift13";
    private readonly ITaypointWillRepository _taypointWillRepository = A.Fake<ITaypointWillRepository>(o => o.Strict());
    private readonly IOptionsMonitor<TaypointWillOptions> _options = A.Fake<IOptionsMonitor<TaypointWillOptions>>(o => o.Strict());
    private readonly RunContext _runContext;
    private readonly TaypointsSuccessionSlashCommand _command;

    public TaypointsSuccessionSlashCommandTests()
    {
        _command = new(_taypointWillRepository, _options, TimeProvider.System);
        _runContext = CommandUtils.CreateTestContext(_command);
    }

    [Fact]
    public async Task GetCommandAsync_WhenNoWills_ThenReturnsSuccessEmbed()
    {
        A.CallTo(() => _taypointWillRepository.GetWillsWithBeneficiaryAsync(_runContext.User)).Returns([]);
        A.CallTo(() => _taypointWillRepository.GetWillAsync(_runContext.User)).Returns(null);
        A.CallTo(() => _options.CurrentValue).Returns(new TaypointWillOptions { DaysOfInactivityBeforeWillCanBeClaimed = 20 });

        var result = (await (await _command.GetCommandAsync(_runContext, new())).RunAsync()).Should().BeOfType<MessageResult>().Which;

        result.Message.Content.Embeds.Should().ContainSingle().Which
            .Color.Should().Be(TaylorBotColors.SuccessColor);
    }

    [Fact]
    public async Task GetCommandAsync_WhenHasWillsToClaim_ThenReturnsClaimEmbed()
    {
        const uint InactiveDaysForClaim = 20;
        A.CallTo(() => _options.CurrentValue).Returns(new TaypointWillOptions { DaysOfInactivityBeforeWillCanBeClaimed = InactiveDaysForClaim });
        var oneDayBeforeThreshold = DateTimeOffset.UtcNow.AddDays(-(InactiveDaysForClaim + 1));
        A.CallTo(() => _taypointWillRepository.GetWillsWithBeneficiaryAsync(_runContext.User)).Returns([
            new WillOwner(
                OwnerUserId: new("1"),
                OwnerUsername: AUsername,
                OwnerLatestSpokeAt: oneDayBeforeThreshold
            )
        ]);

        var result = (await (await _command.GetCommandAsync(_runContext, new())).RunAsync()).Should().BeOfType<MessageResult>().Which;

        result.Message.Content.Embeds.Should().ContainSingle().Which
            .Color.Should().Be(TaylorBotColors.SuccessColor);
        result.Message.Components.Should().ContainSingle().Which
            .components.Should().HaveCount(2, "claim + skip buttons");
    }

    [Fact]
    public async Task GetCommandAsync_WhenHasWill_ThenReturnsSuccessEmbedWithSuccessor()
    {
        A.CallTo(() => _taypointWillRepository.GetWillsWithBeneficiaryAsync(_runContext.User)).Returns([]);
        A.CallTo(() => _taypointWillRepository.GetWillAsync(_runContext.User)).Returns(new Will(new("1"), AUsername));
        A.CallTo(() => _options.CurrentValue).Returns(new TaypointWillOptions { DaysOfInactivityBeforeWillCanBeClaimed = 20 });

        var result = (await (await _command.GetCommandAsync(_runContext, new())).RunAsync()).Should().BeOfType<MessageResult>().Which;

        result.Message.Content.Embeds.Should().ContainSingle().Which
            .Color.Should().Be(TaylorBotColors.SuccessColor);
        result.Message.Components.Should().HaveCount(2);
        result.Message.Components[0].components.Should().HaveCount(1, "user select");
        result.Message.Components[1].components.Should().HaveCount(1, "remove button");
    }
}
