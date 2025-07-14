using FakeItEasy;
using FluentAssertions;
using TaylorBot.Net.Commands.Discord.Program.Modules.RandomGeneration.Commands;
using TaylorBot.Net.Commands.Discord.Program.Tests.Helpers;
using TaylorBot.Net.Core.Random;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests.RandomGeneration;

public class ChooseSlashCommandTests
{
    private readonly ICryptoSecureRandom _cryptoSecureRandom = A.Fake<ICryptoSecureRandom>(o => o.Strict());
    private readonly ChooseSlashCommand _chooseCommand;

    public ChooseSlashCommandTests()
    {
        _chooseCommand = new(_cryptoSecureRandom, CommandUtils.Mentioner);
    }

    [Fact]
    public async Task Choose_ThenReturnsEmbedWithChosenOption()
    {
        const string ChosenOption = "Speak Now";
        A.CallTo(() => _cryptoSecureRandom.GetRandomElement(A<IReadOnlyList<string>>.That.Contains(ChosenOption))).Returns(ChosenOption);

        var result = (EmbedResult)await _chooseCommand.Choose($"Taylor Swift, Fearless, {ChosenOption}, Red, 1989, reputation, Lover", CommandUtils.CreateTestContext(_chooseCommand)).RunAsync();

        result.Embed.Description.Should().Be(ChosenOption);
    }
}
