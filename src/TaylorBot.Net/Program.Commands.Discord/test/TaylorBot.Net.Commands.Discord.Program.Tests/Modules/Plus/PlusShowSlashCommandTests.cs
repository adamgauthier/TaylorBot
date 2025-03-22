﻿using FakeItEasy;
using FluentAssertions;
using TaylorBot.Net.Commands.Discord.Program.Modules.Plus.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Plus.Domain;
using TaylorBot.Net.Commands.Discord.Program.Tests.Helpers;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests.Modules.Plus;

public class PlusShowSlashCommandTests
{
    private readonly IPlusRepository _plusRepository = A.Fake<IPlusRepository>(o => o.Strict());
    private readonly IPlusUserRepository _plusUserRepository = A.Fake<IPlusUserRepository>(o => o.Strict());

    private readonly RunContext _runContext;
    private readonly PlusShowSlashCommand _command;

    public PlusShowSlashCommandTests()
    {
        _command = new(_plusRepository, _plusUserRepository, CommandUtils.Mentioner);
        _runContext = CommandUtils.CreateTestContext(_command, ContextType.DM);
    }

    [Fact]
    public async Task Show_WhenNonPlusUser_ThenReturnsSuccessEmbed()
    {
        A.CallTo(() => _plusUserRepository.GetPlusUserAsync(_runContext.User)).Returns(null);

        var result = (EmbedResult)await (await _command.GetCommandAsync(_runContext, new())).RunAsync();

        result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
    }
}
