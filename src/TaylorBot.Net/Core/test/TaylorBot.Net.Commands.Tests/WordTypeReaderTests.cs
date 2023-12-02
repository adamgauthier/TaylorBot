using Discord.Commands;
using FakeItEasy;
using FluentAssertions;
using TaylorBot.Net.Commands.DiscordNet;
using Xunit;

namespace TaylorBot.Net.Commands.Types.Tests;

public class WordTypeReaderTests
{
    private readonly ITaylorBotCommandContext _commandContext = A.Fake<ITaylorBotCommandContext>(o => o.Strict());
    private readonly IServiceProvider _serviceProvider = A.Fake<IServiceProvider>(o => o.Strict());

    private readonly WordTypeReader _wordTypeReader = new();

    [Fact]
    public async Task ReadAsync_WhenInputContainsSpace_ThenReturnsParseFailed()
    {
        var result = await _wordTypeReader.ReadAsync(_commandContext, "taylor swift", _serviceProvider);

        result.Error.Should().Be(CommandError.ParseFailed);
    }

    [Fact]
    public async Task ReadAsync_WhenInputContainsNewLine_ThenReturnsParseFailed()
    {
        var result = await _wordTypeReader.ReadAsync(_commandContext, "taylor\nswift", _serviceProvider);

        result.Error.Should().Be(CommandError.ParseFailed);
    }

    [Fact]
    public async Task ReadAsync_WhenValidInput_ThenReturnsWordWithInput()
    {
        var input = "tay";

        var result = (Word)(await _wordTypeReader.ReadAsync(_commandContext, input, _serviceProvider)).Values.Single().Value;

        result.Value.Should().Be(input);
    }
}
