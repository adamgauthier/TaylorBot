using Discord;
using FakeItEasy;
using FluentAssertions;
using TaylorBot.Net.Commands.Discord.Program.Modules.DiscordInfo.Commands;
using TaylorBot.Net.Commands.Discord.Program.Services;
using TaylorBot.Net.Commands.Discord.Program.Tests.Helpers;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Types;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests;

public class DiscordInfoModuleTests
{
    private readonly ITaylorBotCommandContext _commandContext = A.Fake<ITaylorBotCommandContext>();
    private readonly ChannelTypeStringMapper _channelTypeStringMapper = new();
    private readonly IUserTracker _userTracker = A.Fake<IUserTracker>(o => o.Strict());
    private readonly DiscordInfoModule _discordInfoModule;

    public DiscordInfoModuleTests()
    {
        _discordInfoModule = new DiscordInfoModule(new SimpleCommandRunner(), _channelTypeStringMapper, _userTracker, new AvatarSlashCommand());
        _discordInfoModule.SetContext(_commandContext);
    }

    [Fact]
    public async Task AvatarAsync_ThenReturnsAvatarEmbed()
    {
        const string AnAvatarId = "1";
        var user = A.Fake<IUser>();
        A.CallTo(() => user.AvatarId).Returns(AnAvatarId);
        var userArgument = A.Fake<IUserArgument<IUser>>();
        A.CallTo(() => userArgument.GetTrackedUserAsync()).Returns(user);

        var result = (await _discordInfoModule.AvatarAsync(userArgument)).GetResult<EmbedResult>();

        result.Embed.Image!.Value.Url.Should().StartWith("https://cdn.discordapp.com/avatars/");
    }

    [Fact]
    public async Task UserInfoAsync_ThenReturnsIdFieldEmbed()
    {
        const ulong AnId = 1;
        var guildUser = A.Fake<IGuildUser>();
        A.CallTo(() => guildUser.Id).Returns(AnId);
        var userArgument = A.Fake<IUserArgument<IGuildUser>>();
        A.CallTo(() => userArgument.GetTrackedUserAsync()).Returns(guildUser);

        var result = (await _discordInfoModule.UserInfoAsync(userArgument)).GetResult<EmbedResult>();

        result.Embed.Fields.Single(f => f.Name == "Id").Value.Should().Contain(AnId.ToString());
    }

    [Fact]
    public async Task RandomUserInfoAsync_ThenReturnsIdFieldEmbedFromGuildUsers()
    {
        const ulong AnId = 1;
        var guildUser = A.Fake<IGuildUser>();
        A.CallTo(() => guildUser.Id).Returns(AnId);
        var guild = A.Fake<IGuild>();
        A.CallTo(() => guild.GetUsersAsync(CacheMode.CacheOnly, null)).Returns(new[] { guildUser });
        A.CallTo(() => _commandContext.Guild).Returns(guild);
        A.CallTo(() => _userTracker.TrackUserFromArgumentAsync(guildUser)).Returns(default);

        var result = (await _discordInfoModule.RandomUserInfoAsync()).GetResult<EmbedResult>();

        result.Embed.Fields.Single(f => f.Name == "Id").Value.Should().Contain(AnId.ToString());
    }

    [Fact]
    public async Task RoleInfoAsync_ThenReturnsIdFieldEmbed()
    {
        const ulong AnId = 1;
        var role = A.Fake<IRole>();
        A.CallTo(() => role.Id).Returns(AnId);

        var result = (await _discordInfoModule.RoleInfoAsync(new RoleArgument<IRole>(role))).GetResult<EmbedResult>();

        result.Embed.Fields.Single(f => f.Name == "Id").Value.Should().Contain(AnId.ToString());
    }

    [Fact]
    public async Task ChannelInfoAsync_ThenReturnsIdFieldEmbed()
    {
        const ulong AnId = 1;
        var channel = A.Fake<ITextChannel>();
        A.CallTo(() => channel.Id).Returns(AnId);

        var result = (await _discordInfoModule.ChannelInfoAsync(new ChannelArgument<IChannel>(channel))).GetResult<EmbedResult>();

        result.Embed.Fields.Single(f => f.Name == "Id").Value.Should().Contain(AnId.ToString());
    }

    [Fact]
    public async Task ServerInfoAsync_ThenReturnsIdFieldEmbed()
    {
        const ulong AnId = 1;
        var guild = A.Fake<IGuild>();
        A.CallTo(() => guild.Id).Returns(AnId);
        A.CallTo(() => guild.VoiceRegionId).Returns("us-east");
        var role = A.Fake<IRole>();
        A.CallTo(() => role.Mention).Returns("<@0>");
        A.CallTo(() => guild.Roles).Returns(new[] { role });
        A.CallTo(() => _commandContext.Guild).Returns(guild);

        var result = (await _discordInfoModule.ServerInfoAsync()).GetResult<EmbedResult>();

        result.Embed.Fields.Single(f => f.Name == "Id").Value.Should().Contain(AnId.ToString());
    }
}
