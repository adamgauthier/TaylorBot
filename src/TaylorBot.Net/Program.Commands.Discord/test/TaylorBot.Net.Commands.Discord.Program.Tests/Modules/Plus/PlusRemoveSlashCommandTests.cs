using FakeItEasy;
using FluentAssertions;
using TaylorBot.Net.Commands.Discord.Program.Modules.Plus.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Plus.Domain;
using TaylorBot.Net.Commands.Discord.Program.Tests.Helpers;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests.Modules.Plus;

public class PlusRemoveSlashCommandTests
{
    private readonly IPlusRepository _plusRepository = A.Fake<IPlusRepository>(o => o.Strict());
    private readonly IPlusUserRepository _plusUserRepository = A.Fake<IPlusUserRepository>(o => o.Strict());

    private readonly RunContext _runContext;
    private readonly PlusRemoveSlashCommand _command;

    public PlusRemoveSlashCommandTests()
    {
        _command = new(_plusRepository, _plusUserRepository);
        _runContext = CommandUtils.CreateTestContext(_command);
    }

    [Fact]
    public async Task RemoveAsync_ThenReturnsSuccessEmbed()
    {
        A.CallTo(() => _plusUserRepository.DisablePlusGuildAsync(_runContext.User, _runContext.Guild!)).Returns(new ValueTask());

        var result = (EmbedResult)await (await _command.GetCommandAsync(_runContext, new())).RunAsync();

        result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
    }
}
