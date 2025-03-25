﻿using Discord;
using FakeItEasy;
using FluentAssertions;
using TaylorBot.Net.Commands.Discord.Program.Modules.UsernameHistory.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.UsernameHistory.Domain;
using TaylorBot.Net.Commands.Discord.Program.Tests.Helpers;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Core.Colors;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests.Modules.UsernameHistory;

public class UsernamesModuleTests
{
    private readonly IUser _commandUser = A.Fake<IUser>();
    private readonly IUserMessage _message = A.Fake<IUserMessage>();
    private readonly IMessageChannel _channel = A.Fake<ITextChannel>();
    private readonly ITaylorBotCommandContext _commandContext = A.Fake<ITaylorBotCommandContext>();
    private readonly IUsernameHistoryRepository _usernameHistoryRepository = A.Fake<IUsernameHistoryRepository>(o => o.Strict());
    private readonly UsernamesModule _usernamesModule;

    public UsernamesModuleTests()
    {
        _usernamesModule = new UsernamesModule(new SimpleCommandRunner(), new UsernamesShowSlashCommand(_usernameHistoryRepository, CommandUtils.Mentioner));
        _usernamesModule.SetContext(_commandContext);
        A.CallTo(() => _commandContext.User).Returns(_commandUser);
        A.CallTo(() => _commandContext.Channel).Returns(_channel);
        A.CallTo(() => _commandContext.CommandPrefix).Returns("!");
        A.CallTo(() => _message.Channel).Returns(_channel);
    }

    [Fact]
    public async Task GetAsync_WhenUsernameHistoryPrivate_ThenReturnsSuccessEmbed()
    {
        A.CallTo(() => _usernameHistoryRepository.IsUsernameHistoryHiddenFor(new(_commandUser))).Returns(true);

        var result = (await _usernamesModule.GetAsync()).GetResult<EmbedResult>();

        result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
    }

    [Fact]
    public async Task GetAsync_WhenUsernameHistoryPublic_ThenReturnsPageEmbedWithUsername()
    {
        const string AUsername = "Enchanted13";

        A.CallTo(() => _usernameHistoryRepository.IsUsernameHistoryHiddenFor(new(_commandUser))).Returns(false);
        A.CallTo(() => _usernameHistoryRepository.GetUsernameHistoryFor(new(_commandUser), 75)).Returns([
            new UsernameChange(Username: AUsername, ChangedAt: DateTimeOffset.UtcNow.AddDays(-1))
        ]);

        var result = (await _usernamesModule.GetAsync()).GetResult<PageMessageResult>();
        using var _ = await result.PageMessage.SendAsync(_commandUser, _message);

        A.CallTo(() => _channel.SendMessageAsync(
            null,
            false,
            A<Embed>.That.Matches(e => e.Description.Contains(AUsername)),
            null,
            A<AllowedMentions>.Ignored,
            A<MessageReference>.Ignored,
            null,
            null,
            null,
            MessageFlags.None,
            null
        )).MustHaveHappenedOnceExactly();
    }
}
