﻿using Discord;
using FakeItEasy;
using FluentAssertions;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Domain;
using TaylorBot.Net.Core.Colors;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests;

public class LastFmSetCommandTests
{
    private readonly IUser _commandUser = A.Fake<IUser>();
    private readonly ILastFmUsernameRepository _lastFmUsernameRepository = A.Fake<ILastFmUsernameRepository>(o => o.Strict());
    private readonly LastFmSetCommand _lastFmSetCommand;

    public LastFmSetCommandTests()
    {
        _lastFmSetCommand = new(_lastFmUsernameRepository);
    }

    [Fact]
    public async Task Set_ThenReturnsSuccessEmbed()
    {
        var lastFmUsername = new LastFmUsername("taylorswift");
        A.CallTo(() => _lastFmUsernameRepository.SetLastFmUsernameAsync(_commandUser, lastFmUsername)).Returns(default);

        var result = (EmbedResult)await _lastFmSetCommand.Set(_commandUser, lastFmUsername, isLegacyCommand: false).RunAsync();

        result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
    }
}
