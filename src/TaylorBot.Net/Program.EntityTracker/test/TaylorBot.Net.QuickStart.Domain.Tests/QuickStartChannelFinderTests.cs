using Discord;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace TaylorBot.Net.QuickStart.Domain.Tests;

public class QuickStartChannelFinderTests
{
    private readonly QuickStartChannelFinder _quickStartChannelFinder = new();

    [Fact]
    public async Task FindQuickStartChannelAsync_WhenMultipleAvailableChannels_ThenReturnsChannelNamedGeneral()
    {
        var botUser = A.Fake<IGuildUser>(o => o.Strict());
        var everyone = A.Fake<IRole>(o => o.Strict());
        var guild = A.Fake<IGuild>(o => o.Strict());
        A.CallTo(() => guild.EveryoneRole).Returns(everyone);
        A.CallTo(() => guild.GetCurrentUserAsync(CacheMode.AllowDownload, null)).Returns(botUser);

        var generalChannel = CreateAvailableChannel("general", botUser, everyone);

        A.CallTo(() => guild.GetTextChannelsAsync(CacheMode.AllowDownload, null)).Returns(new[] {
            CreateAvailableChannel("memes", botUser, everyone),
            generalChannel,
            CreateAvailableChannel("serious", botUser, everyone),
            CreateAvailableChannel("commands", botUser, everyone)
        });


        var channel = await _quickStartChannelFinder.FindQuickStartChannelAsync<IGuild, ITextChannel>(guild);


        channel.Should().Be(generalChannel);
    }

    private ITextChannel CreateAvailableChannel(string name, IGuildUser botUser, IRole everyone)
    {
        var channel = A.Fake<ITextChannel>(o => o.Strict());
        A.CallTo(() => channel.Name).Returns(name);
        A.CallTo(() => channel.GetPermissionOverwrite(everyone)).Returns(null);
        A.CallTo(() => botUser.GetPermissions(channel)).Returns(new ChannelPermissions(sendMessages: true));
        A.CallTo(() => channel.Equals(A<ITextChannel>.That.IsSameAs(channel))).Returns(true);
        A.CallTo(() => channel.Equals(A<ITextChannel>.That.Not.IsSameAs(channel))).Returns(false);
        return channel;
    }
}
