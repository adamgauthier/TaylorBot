﻿using Discord;
using FakeItEasy;
using FluentAssertions;
using TaylorBot.Net.Commands.Discord.Program.Modules.RandomGeneration.Commands;
using TaylorBot.Net.Commands.Discord.Program.Tests.Helpers;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Core.Random;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests;

public class RandomModuleTests
{
    private readonly IUser _commandUser = A.Fake<IUser>();
    private readonly ITaylorBotCommandContext _commandContext = A.Fake<ITaylorBotCommandContext>();
    private readonly IMessageChannel _channel = A.Fake<ITextChannel>();
    private readonly ICryptoSecureRandom _cryptoSecureRandom = A.Fake<ICryptoSecureRandom>(o => o.Strict());
    private readonly RandomModule _randomModule;

    public RandomModuleTests()
    {
        _randomModule = new RandomModule(new SimpleCommandRunner(), new(new SimpleCommandRunner()), new ChooseSlashCommand(_cryptoSecureRandom, CommandUtils.Mentioner));
        _randomModule.SetContext(_commandContext);
        A.CallTo(() => _commandContext.Channel).Returns(_channel);
        A.CallTo(() => _commandContext.User).Returns(_commandUser);
    }

    [Fact]
    public async Task ChooseAsync_ThenReturnsEmbedWithChosenOption()
    {
        const string ChosenOption = "Speak Now";
        A.CallTo(() => _cryptoSecureRandom.GetRandomElement(A<IReadOnlyList<string>>.That.Contains(ChosenOption))).Returns(ChosenOption);

        var result = (await _randomModule.ChooseAsync($"Taylor Swift, Fearless, {ChosenOption}, Red, 1989, reputation, Lover")).GetResult<EmbedResult>();

        result.Embed.Description.Should().Contain(ChosenOption);
    }
}
