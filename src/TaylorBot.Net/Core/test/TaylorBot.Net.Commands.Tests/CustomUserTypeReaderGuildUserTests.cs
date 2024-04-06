using Discord;
using FakeItEasy;
using FluentAssertions;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.Core.User;
using Xunit;

namespace TaylorBot.Net.Commands.Tests;

public class CustomUserTypeReaderGuildUserTests
{
    private const ulong AnId = 1;
    private const string AUsername = "TaylorSwift13";
    private const string ANickname = "Taylor";
    private const ulong AnotherId = 1989;
    private const string AnotherUsername = "WelcomeToNewYork";
    private const string AnotherNickname = "Joe";
    private static readonly IGuildUser AGuildUser = A.Fake<IGuildUser>();

    private readonly IMessageChannel _channel = A.Fake<IMessageChannel>(o => o.Strict());
    private readonly ITaylorBotCommandContext _commandContext = A.Fake<ITaylorBotCommandContext>(o => o.Strict());
    private readonly IServiceProvider _serviceProvider = A.Fake<IServiceProvider>(o => o.Strict());
    private readonly IUserTracker _userTracker = A.Fake<IUserTracker>(o => o.Strict());
    private readonly MentionedUserTypeReader<IGuildUser> _mentionedUserTypeReader;

    private readonly CustomUserTypeReader<IGuildUser> _customUserTypeReader;

    public CustomUserTypeReaderGuildUserTests()
    {
        A.CallTo(() => _commandContext.Channel).Returns(_channel);
        A.CallTo(() => AGuildUser.Id).Returns(AnId);
        _mentionedUserTypeReader = new MentionedUserTypeReader<IGuildUser>(_userTracker);
        _customUserTypeReader = new CustomUserTypeReader<IGuildUser>(_mentionedUserTypeReader, _userTracker);
    }

    [Fact]
    public async Task ReadAsync_WhenGuildUserMentionInGuild_ThenReturnsGuildUser()
    {
        var guild = A.Fake<IGuild>(o => o.Strict());
        A.CallTo(() => _commandContext.Guild).Returns(guild);
        A.CallTo(() => guild.GetUsersAsync(CacheMode.CacheOnly, null)).Returns([]);
        A.CallTo(() => _channel.GetUsersAsync(CacheMode.CacheOnly, null)).Returns(AsyncEnumerable.Empty<IReadOnlyCollection<IGuildUser>>());
        var taylorbotClient = A.Fake<ITaylorBotClient>(o => o.Strict());
        A.CallTo(() => taylorbotClient.ResolveGuildUserAsync(guild, A<SnowflakeId>.That.Matches(id => id.Id == AnId))).Returns(AGuildUser);
        A.CallTo(() => _serviceProvider.GetService(typeof(ITaylorBotClient))).Returns(taylorbotClient);
        A.CallTo(() => _userTracker.TrackUserFromArgumentAsync(A<DiscordUser>.That.Matches(u => u.Id == AnId))).Returns(default);

        var result = (UserArgument<IGuildUser>)(await _customUserTypeReader.ReadAsync(_commandContext, MentionUtils.MentionUser(AnId), _serviceProvider)).Values.Single().Value;
        var user = await result.GetTrackedUserAsync();

        user.Should().Be(AGuildUser);
    }

    [Fact]
    public async Task ReadAsync_WhenInputIsGuildUserUsername_ThenReturnsGuildUser()
    {
        var guild = A.Fake<IGuild>(o => o.Strict());
        A.CallTo(() => _commandContext.Guild).Returns(guild);
        var guildUser = CreateFakeGuildUser(AnId, AUsername, ANickname);
        var anotherGuildUser = CreateFakeGuildUser(AnotherId, AnotherUsername, AnotherNickname);
        A.CallTo(() => guild.GetUsersAsync(CacheMode.CacheOnly, null)).Returns([guildUser, anotherGuildUser]);
        A.CallTo(() => _channel.GetUsersAsync(CacheMode.CacheOnly, null)).Returns(AsyncEnumerable.Empty<IReadOnlyCollection<IGuildUser>>());

        var result = (UserArgument<IGuildUser>)(await _customUserTypeReader.ReadAsync(_commandContext, AUsername, _serviceProvider)).Values.Single().Value;
        var user = await result.GetTrackedUserAsync();

        user.Should().Be(guildUser);
    }

    [Fact]
    public async Task ReadAsync_WhenInputIsGuildUserNickname_ThenReturnsGuildUser()
    {
        var guild = A.Fake<IGuild>(o => o.Strict());
        A.CallTo(() => _commandContext.Guild).Returns(guild);
        var guildUser = CreateFakeGuildUser(AnId, AUsername, ANickname);
        var anotherGuildUser = CreateFakeGuildUser(AnotherId, AnotherUsername, AnotherNickname);
        A.CallTo(() => guild.GetUsersAsync(CacheMode.CacheOnly, null)).Returns([guildUser, anotherGuildUser]);
        A.CallTo(() => _channel.GetUsersAsync(CacheMode.CacheOnly, null)).Returns(AsyncEnumerable.Empty<IReadOnlyCollection<IGuildUser>>());

        var result = (UserArgument<IGuildUser>)(await _customUserTypeReader.ReadAsync(_commandContext, AnotherNickname, _serviceProvider)).Values.Single().Value;
        var user = await result.GetTrackedUserAsync();

        user.Should().Be(anotherGuildUser);
    }

    [Fact]
    public async Task ReadAsync_WhenInputIsGuildUserNicknameAndInUsernameButStartOfAnotherUsername_ThenBestMatchIsGuildUser()
    {
        var guild = A.Fake<IGuild>(o => o.Strict());
        A.CallTo(() => _commandContext.Guild).Returns(guild);
        var guildUser = CreateFakeGuildUser(AnId, username: "ImATaylorFan89", nickname: "Tay");
        var anotherGuildUser = CreateFakeGuildUser(AnotherId, username: "TaylorSwift13", nickname: "Emelie");
        A.CallTo(() => guild.GetUsersAsync(CacheMode.CacheOnly, null)).Returns([guildUser, anotherGuildUser]);
        A.CallTo(() => _channel.GetUsersAsync(CacheMode.CacheOnly, null)).Returns(AsyncEnumerable.Empty<IReadOnlyCollection<IGuildUser>>());

        var result = (UserArgument<IGuildUser>)(await _customUserTypeReader.ReadAsync(_commandContext, "tay", _serviceProvider)).BestMatch;
        var user = await result.GetTrackedUserAsync();

        user.Should().Be(guildUser);
    }

    private IGuildUser CreateFakeGuildUser(ulong id, string username, string nickname)
    {
        var guildUser = A.Fake<IGuildUser>();
        A.CallTo(() => guildUser.Id).Returns(id);
        A.CallTo(() => guildUser.Username).Returns(username);
        A.CallTo(() => guildUser.Nickname).Returns(nickname);
        A.CallTo(() => _userTracker.TrackUserFromArgumentAsync(A<DiscordUser>.That.Matches(u => u.Id == id))).Returns(default);
        return guildUser;
    }
}
