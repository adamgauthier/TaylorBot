using Discord;
using Discord.Commands;
using FakeItEasy;
using FluentAssertions;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.Core.User;
using Xunit;

namespace TaylorBot.Net.Commands.Types.Tests;

public class MentionedUserTypeReaderTests
{
    private const ulong AnId = 1;
    private static readonly IUser AUser = A.Fake<IUser>();
    private static readonly IGuildUser AGuildUser = A.Fake<IGuildUser>();

    private readonly ITaylorBotCommandContext _commandContext = A.Fake<ITaylorBotCommandContext>(o => o.Strict());
    private readonly IServiceProvider _serviceProvider = A.Fake<IServiceProvider>(o => o.Strict());
    private readonly IUserTracker _userTracker = A.Fake<IUserTracker>(o => o.Strict());

    public MentionedUserTypeReaderTests()
    {
        A.CallTo(() => AUser.Id).Returns(AnId);
        A.CallTo(() => AGuildUser.Id).Returns(AnId);
    }

    [Fact]
    public async Task ReadAsync_WhenIUserMentionInChannel_ThenReturnsUser()
    {
        var mentionedUserTypeReader = new MentionedUserTypeReader<IUser>(_userTracker);
        var channel = A.Fake<IMessageChannel>(o => o.Strict());
        A.CallTo(() => _commandContext.Guild).Returns(null!);
        A.CallTo(() => _commandContext.Channel).Returns(channel);
        A.CallTo(() => channel.GetUserAsync(AnId, CacheMode.CacheOnly, null)).Returns(AUser);
        A.CallTo(() => _userTracker.TrackUserFromArgumentAsync(A<DiscordUser>.That.Matches(u => u.Id == AnId))).Returns(default);

        var result = (IMentionedUser<IUser>)(await mentionedUserTypeReader.ReadAsync(_commandContext, MentionUtils.MentionUser(AnId), _serviceProvider)).Values.Single().Value;
        var user = await result.GetTrackedUserAsync();

        user.Should().Be(AUser);
    }

    [Fact]
    public async Task ReadAsync_WhenIGuildUserNotAMention_ThenReturnsParseFailed()
    {
        var mentionedUserTypeReader = new MentionedUserTypeReader<IGuildUser>(_userTracker);

        var result = await mentionedUserTypeReader.ReadAsync(_commandContext, "Taylor Swift", _serviceProvider);

        result.Error.Should().Be(CommandError.ParseFailed);
    }

    [Fact]
    public async Task ReadAsync_WhenIGuildUserMentionNotInGuild_ThenReturnsParseFailed()
    {
        var mentionedUserTypeReader = new MentionedUserTypeReader<IGuildUser>(_userTracker);
        var guild = A.Fake<IGuild>(o => o.Strict());
        A.CallTo(() => _commandContext.Guild).Returns(guild);
        var taylorbotClient = A.Fake<ITaylorBotClient>(o => o.Strict());
        A.CallTo(() => taylorbotClient.ResolveGuildUserAsync(guild, A<SnowflakeId>.That.Matches(id => id.Id == AnId))).Returns(null);
        A.CallTo(() => _serviceProvider.GetService(typeof(ITaylorBotClient))).Returns(taylorbotClient);
        A.CallTo(() => _userTracker.TrackUserFromArgumentAsync(A<DiscordUser>.That.Matches(u => u.Id == AnId))).Returns(default);

        var result = await mentionedUserTypeReader.ReadAsync(_commandContext, MentionUtils.MentionUser(AnId), _serviceProvider);

        result.Error.Should().Be(CommandError.ParseFailed);
    }

    [Fact]
    public async Task ReadAsync_WhenIGuildUserMentionInGuild_ThenReturnsIGuildUser()
    {
        var mentionedUserTypeReader = new MentionedUserTypeReader<IGuildUser>(_userTracker);
        var guild = A.Fake<IGuild>(o => o.Strict());
        A.CallTo(() => _commandContext.Guild).Returns(guild);
        var taylorbotClient = A.Fake<ITaylorBotClient>(o => o.Strict());
        A.CallTo(() => taylorbotClient.ResolveGuildUserAsync(guild, A<SnowflakeId>.That.Matches(id => id.Id == AnId))).Returns(AGuildUser);
        A.CallTo(() => _serviceProvider.GetService(typeof(ITaylorBotClient))).Returns(taylorbotClient);
        A.CallTo(() => _userTracker.TrackUserFromArgumentAsync(A<DiscordUser>.That.Matches(u => u.Id == AnId))).Returns(default);

        var result = (IMentionedUser<IGuildUser>)(await mentionedUserTypeReader.ReadAsync(_commandContext, MentionUtils.MentionUser(AnId), _serviceProvider)).Values.Single().Value;
        var user = await result.GetTrackedUserAsync();

        user.Should().Be(AGuildUser);
    }
}
