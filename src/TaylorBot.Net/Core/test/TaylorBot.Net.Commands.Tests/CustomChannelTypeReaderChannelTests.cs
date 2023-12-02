using Discord;
using Discord.Commands;
using FakeItEasy;
using FluentAssertions;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Types;
using Xunit;

namespace TaylorBot.Net.Commands.Tests;

public class CustomChannelTypeReaderChannelTests
{
    private const ulong AnId = 1;
    private const ulong AnotherId = 1989;
    private const string AName = "general";

    private readonly IGuild _guild = A.Fake<IGuild>(o => o.Strict());
    private readonly ITaylorBotCommandContext _commandContext = A.Fake<ITaylorBotCommandContext>(o => o.Strict());
    private readonly IServiceProvider _serviceProvider = A.Fake<IServiceProvider>(o => o.Strict());

    private readonly CustomChannelTypeReader<IChannel> _customChannelTypeReader;

    public CustomChannelTypeReaderChannelTests()
    {
        _customChannelTypeReader = new CustomChannelTypeReader<IChannel>();
    }

    [Fact]
    public async Task ReadAsync_WhenInputIsChannelMentionNotInGuild_ThenReturnsObjectNotFound()
    {
        var guild = A.Fake<IGuild>(o => o.Strict());
        var channel = CreateFakeTextChannel(AnId, AName);
        A.CallTo(() => _commandContext.Guild).Returns(guild);
        A.CallTo(() => guild.GetChannelsAsync(CacheMode.CacheOnly, null)).Returns(new[] { channel });
        var guildUser = A.Fake<IGuildUser>(o => o.Strict());
        A.CallTo(() => _commandContext.User).Returns(guildUser);
        A.CallTo(() => guildUser.GetPermissions(channel)).Returns(ChannelPermissions.All(channel));

        var result = await _customChannelTypeReader.ReadAsync(_commandContext, MentionUtils.MentionChannel(AnotherId), _serviceProvider);

        result.Error.Should().Be(CommandError.ObjectNotFound);
    }

    [Fact]
    public async Task ReadAsync_WhenInputIsChannelIdNotInGuild_ThenReturnsObjectNotFound()
    {
        var guild = A.Fake<IGuild>(o => o.Strict());
        var channel = CreateFakeTextChannel(AnId, AName);
        A.CallTo(() => _commandContext.Guild).Returns(guild);
        A.CallTo(() => guild.GetChannelsAsync(CacheMode.CacheOnly, null)).Returns(new[] { channel });
        var guildUser = A.Fake<IGuildUser>(o => o.Strict());
        A.CallTo(() => _commandContext.User).Returns(guildUser);
        A.CallTo(() => guildUser.GetPermissions(channel)).Returns(ChannelPermissions.All(channel));

        var result = await _customChannelTypeReader.ReadAsync(_commandContext, $"{AnotherId}", _serviceProvider);

        result.Error.Should().Be(CommandError.ObjectNotFound);
    }

    private ITextChannel CreateFakeTextChannel(ulong id, string name)
    {
        var channel = A.Fake<ITextChannel>(o => o.Strict());
        A.CallTo(() => channel.Id).Returns(id);
        A.CallTo(() => channel.Name).Returns(AName);
        return channel;
    }
}
