using Discord;
using FakeItEasy;
using FluentAssertions;
using TaylorBot.Net.Commands.Discord.Program.Modules.DiscordInfo.Commands;
using TaylorBot.Net.Commands.Discord.Program.Tests.Helpers;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Types;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests;

public class DiscordInfoModuleTests
{
    private readonly ITaylorBotCommandContext _commandContext = A.Fake<ITaylorBotCommandContext>();
    private readonly IMessageChannel _channel = A.Fake<ITextChannel>();
    private readonly DiscordInfoModule _discordInfoModule;

    public DiscordInfoModuleTests()
    {
        _discordInfoModule = new DiscordInfoModule(new SimpleCommandRunner(), new AvatarSlashCommand());
        _discordInfoModule.SetContext(_commandContext);
        A.CallTo(() => _commandContext.Channel).Returns(_channel);
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
    public async Task RoleInfoAsync_ThenReturnsIdFieldEmbed()
    {
        const ulong AnId = 1;
        var role = A.Fake<IRole>();
        A.CallTo(() => role.Id).Returns(AnId);

        var result = (await _discordInfoModule.RoleInfoAsync(new RoleArgument<IRole>(role))).GetResult<EmbedResult>();

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
        A.CallTo(() => guild.Roles).Returns([role]);
        A.CallTo(() => _commandContext.Guild).Returns(guild);

        var result = (await _discordInfoModule.ServerInfoAsync()).GetResult<EmbedResult>();

        result.Embed.Fields.Single(f => f.Name == "Id").Value.Should().Contain(AnId.ToString());
    }
}
