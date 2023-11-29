using FakeItEasy;
using FluentAssertions;
using TaylorBot.Net.Commands.Discord.Program.Modules.RandomGeneration.Commands;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests.RandomGeneration;

public class ChooseCommandTests
{
    private readonly ICryptoSecureRandom _cryptoSecureRandom = A.Fake<ICryptoSecureRandom>(o => o.Strict());
    private readonly ChooseCommand _chooseCommand;

    public ChooseCommandTests()
    {
        _chooseCommand = new ChooseCommand(_cryptoSecureRandom);
    }

    [Fact]
    public async Task Choose_ThenReturnsEmbedWithChosenOption()
    {
        const string ChosenOption = "Speak Now";
        A.CallTo(() => _cryptoSecureRandom.GetRandomElement(A<IReadOnlyList<string>>.That.Contains(ChosenOption))).Returns(ChosenOption);

        var result = (EmbedResult)await _chooseCommand.Choose($"Taylor Swift, Fearless, {ChosenOption}, Red, 1989, reputation, Lover").RunAsync();

        result.Embed.Description.Should().Be(ChosenOption);
    }
}
