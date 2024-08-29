using Discord;
using FakeItEasy;
using FluentAssertions;
using TaylorBot.Net.Commands.Discord.Program.Modules.DiscordInfo.Commands;
using TaylorBot.Net.Commands.Discord.Program.Tests.Helpers;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Types;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests.Modules.DiscordInfo;

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

        result.Embed.Image.Should().NotBeNull()
            .And.BeOfType<EmbedImage>().Which.Url.Should().StartWith("https://cdn.discordapp.com/avatars/");
    }
}
