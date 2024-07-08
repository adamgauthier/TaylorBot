using FakeItEasy;
using FluentAssertions;
using TaylorBot.Net.Commands.Discord.Program.Modules.Server.Commands;
using TaylorBot.Net.Commands.Discord.Program.Tests.Helpers;
using TaylorBot.Net.Core.Random;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests.RandomGeneration;

public class DiceSlashCommandTests
{
    private readonly ICryptoSecureRandom _cryptoSecureRandom = A.Fake<ICryptoSecureRandom>(o => o.Strict());
    private readonly RunContext _runContext;
    private readonly DiceSlashCommand _command;

    public DiceSlashCommandTests()
    {
        _command = new(_cryptoSecureRandom);
        _runContext = CommandUtils.CreateTestContext(_command);
    }

    [Fact]
    public async Task DiceAsync_ThenReturnsEmbedWithRoll()
    {
        const int FaceCount = 6;
        const int Roll = 2;
        A.CallTo(() => _cryptoSecureRandom.GetInt32(1, FaceCount)).Returns(Roll);

        var result = (EmbedResult)await (await _command.GetCommandAsync(_runContext, new(new(FaceCount)))).RunAsync();

        result.Embed.Description.Should().Contain($"{Roll}");
    }
}
