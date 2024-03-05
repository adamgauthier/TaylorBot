using Discord;
using FakeItEasy;
using FluentAssertions;
using TaylorBot.Net.Commands.Discord.Program.Modules.UsernameHistory.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.UsernameHistory.Domain;
using TaylorBot.Net.Core.Colors;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests.Modules.UsernameHistory;

public class UsernamesVisibilitySlashCommandTests
{
    private readonly IUser _commandUser = A.Fake<IUser>();
    private readonly IGuild _commandGuild = A.Fake<IGuild>();
    private readonly IUsernameHistoryRepository _usernameHistoryRepository = A.Fake<IUsernameHistoryRepository>(o => o.Strict());
    private readonly RunContext _runContext;
    private readonly UsernamesVisibilitySlashCommand _command;

    public UsernamesVisibilitySlashCommandTests()
    {
        _command = new UsernamesVisibilitySlashCommand(_usernameHistoryRepository);
        _runContext = new RunContext(DateTimeOffset.UtcNow, _commandUser, null!, _commandGuild, null!, null!, new("123", _command.Info.Name), null!, null!, null!);
    }

    [Fact]
    public async Task PrivateAsync_ThenReturnsSuccessEmbed()
    {
        var hideHistoryCall = A.CallTo(() => _usernameHistoryRepository.HideUsernameHistoryFor(_commandUser));
        hideHistoryCall.Returns(default);

        var result = (EmbedResult)await (await _command.GetCommandAsync(_runContext, new(new("private")))).RunAsync();

        hideHistoryCall.MustHaveHappenedOnceExactly();
        result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
    }

    [Fact]
    public async Task PublicAsync_ThenReturnsSuccessEmbed()
    {
        var unHideHistoryCall = A.CallTo(() => _usernameHistoryRepository.UnhideUsernameHistoryFor(_commandUser));
        unHideHistoryCall.Returns(default);

        var result = (EmbedResult)await (await _command.GetCommandAsync(_runContext, new(new("public")))).RunAsync();

        unHideHistoryCall.MustHaveHappenedOnceExactly();
        result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
    }
}
