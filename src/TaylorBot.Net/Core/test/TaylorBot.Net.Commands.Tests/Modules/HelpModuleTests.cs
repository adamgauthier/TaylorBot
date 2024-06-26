﻿using Discord;
using Discord.Commands;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Options;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Commands.Tests.Helpers;
using TaylorBot.Net.Core.Colors;
using Xunit;

namespace TaylorBot.Net.Commands.Tests.Modules;

public class HelpModuleTests
{
    private readonly IUser _commandUser = A.Fake<IUser>();
    private readonly IMessageChannel _channel = A.Fake<ITextChannel>();
    private readonly ITaylorBotCommandContext _commandContext = A.Fake<ITaylorBotCommandContext>();
    private readonly IDisabledCommandRepository _disabledCommandRepository = A.Fake<IDisabledCommandRepository>(o => o.Strict());
    private readonly ICommandRepository _commandRepository = A.Fake<ICommandRepository>(o => o.Strict());
    private readonly IOptionsMonitor<CommandApplicationOptions> _commandApplicationOptions = A.Fake<IOptionsMonitor<CommandApplicationOptions>>(o => o.Strict());

    private readonly HelpModule _helpModule;

    public HelpModuleTests()
    {
        _helpModule = new HelpModule(new CommandService(), _disabledCommandRepository, _commandRepository, _commandApplicationOptions, new SimpleCommandRunner());
        _helpModule.SetContext(_commandContext);
        A.CallTo(() => _commandContext.Channel).Returns(_channel);
        A.CallTo(() => _commandContext.User).Returns(_commandUser);
        A.CallTo(() => _commandContext.CommandPrefix).Returns("!");
    }

    [Fact]
    public async Task DiagnosticAsync_WhenOtherApplicationName_ThenReturnsEmptyResult()
    {
        A.CallTo(() => _commandApplicationOptions.CurrentValue).Returns(new CommandApplicationOptions { ApplicationName = "commands-discord" });

        var result = (TaylorBotResult)await _helpModule.DiagnosticAsync("typescript");

        result.Result.Should().BeOfType<EmptyResult>();
    }

    [Fact]
    public async Task DiagnosticAsync_WhenApplicationName_ThenReturnsSuccessEmbed()
    {
        const string ApplicationName = "commands-discord";
        A.CallTo(() => _commandApplicationOptions.CurrentValue).Returns(new CommandApplicationOptions { ApplicationName = ApplicationName });
        var discordClient = A.Fake<IDiscordClient>(o => o.Strict());
        A.CallTo(() => _commandContext.Client).Returns(discordClient);
        A.CallTo(() => _commandContext.CurrentUser).Returns(A.Fake<ISelfUser>());
        A.CallTo(() => discordClient.GetGuildsAsync(CacheMode.CacheOnly, null)).Returns(Array.Empty<IGuild>());
        A.CallTo(() => discordClient.GetDMChannelsAsync(CacheMode.CacheOnly, null)).Returns(Array.Empty<IDMChannel>());

        var result = (await _helpModule.DiagnosticAsync(ApplicationName)).GetResult<EmbedResult>();

        result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
    }
}
