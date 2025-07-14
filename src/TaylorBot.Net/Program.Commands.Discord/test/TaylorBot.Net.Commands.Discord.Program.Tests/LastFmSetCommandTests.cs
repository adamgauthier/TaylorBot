using FakeItEasy;
using FluentAssertions;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Domain;
using TaylorBot.Net.Commands.Discord.Program.Tests.Helpers;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.User;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests;

public class LastFmSetCommandTests
{
    private readonly DiscordUser _commandUser = CommandUtils.AUser;
    private readonly ILastFmUsernameRepository _lastFmUsernameRepository = A.Fake<ILastFmUsernameRepository>(o => o.Strict());
    private readonly LastFmSetSlashCommand _lastFmSetCommand;

    public LastFmSetCommandTests()
    {
        _lastFmSetCommand = new(_lastFmUsernameRepository, CommandUtils.Mentioner);
    }

    [Fact]
    public async Task Set_ThenReturnsSuccessEmbed()
    {
        LastFmUsername lastFmUsername = new("taylorswift");
        A.CallTo(() => _lastFmUsernameRepository.SetLastFmUsernameAsync(_commandUser, lastFmUsername)).Returns(default);

        var result = (EmbedResult)await _lastFmSetCommand.Set(_commandUser, lastFmUsername, CommandUtils.CreateTestContext(_lastFmSetCommand)).RunAsync();

        result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
    }
}
