using FakeItEasy;
using FluentAssertions;
using TaylorBot.Net.Commands.Discord.Program.Modules.UsernameHistory.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.UsernameHistory.Domain;
using TaylorBot.Net.Commands.Discord.Program.Tests.Helpers;
using TaylorBot.Net.Core.Colors;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests.Modules.UsernameHistory;

public class UsernamesShowSlashCommandTests
{
    private readonly IUsernameHistoryRepository _usernameHistoryRepository = A.Fake<IUsernameHistoryRepository>(o => o.Strict());
    private readonly RunContext _runContext;
    private readonly UsernamesShowSlashCommand _command;

    public UsernamesShowSlashCommandTests()
    {
        _command = new UsernamesShowSlashCommand(_usernameHistoryRepository);
        _runContext = CommandUtils.CreateTestContext(_command);
    }

    [Fact]
    public async Task GetAsync_WhenUsernameHistoryPrivate_ThenReturnsSuccessEmbed()
    {
        A.CallTo(() => _usernameHistoryRepository.IsUsernameHistoryHiddenFor(_runContext.User)).Returns(true);

        var result = (EmbedResult)await (await _command.GetCommandAsync(_runContext, new(new(_runContext.User)))).RunAsync();

        result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
        result.Embed.Description.Should().Contain("private");
    }

    [Fact]
    public async Task GetAsync_WhenUsernameHistoryPublic_ThenReturnsPageEmbedWithUsername()
    {
        const string AUsername = "Enchanted13";

        A.CallTo(() => _usernameHistoryRepository.IsUsernameHistoryHiddenFor(_runContext.User)).Returns(false);
        A.CallTo(() => _usernameHistoryRepository.GetUsernameHistoryFor(_runContext.User, 75)).Returns(new[] {
            new UsernameChange(Username: AUsername, ChangedAt: DateTimeOffset.Now.AddDays(-1))
        });

        var result = (MessageResult)await (await _command.GetCommandAsync(_runContext, new(new(_runContext.User)))).RunAsync();

        result.Content.Embeds.Should().ContainSingle().Which
            .Description.Should().Contain(AUsername);
    }
}
