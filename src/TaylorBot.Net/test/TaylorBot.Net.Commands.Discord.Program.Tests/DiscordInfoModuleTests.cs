using Discord;
using FakeItEasy;
using FluentAssertions;
using System.Reflection;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests
{
    public class DiscordInfoModuleTests
    {
        private readonly ITaylorBotCommandContext _commandContext;
        private readonly DiscordInfoModule _discordInfoModule;

        public DiscordInfoModuleTests()
        {
            _commandContext = A.Fake<ITaylorBotCommandContext>(o => o.Strict());

            _discordInfoModule = new DiscordInfoModule();
            var setContext = _discordInfoModule.GetType().GetMethod("Discord.Commands.IModuleBase.SetContext", BindingFlags.Instance | BindingFlags.NonPublic);
            setContext.Invoke(_discordInfoModule, new object[] { _commandContext });
        }

        [Fact]
        public async Task AvatarAsync_ThenReturnsAvatarEmbed()
        {
            const string AnAvatarURL = "https://cdn.discordapp.com/avatars/1/1.png";
            var user = A.Fake<IUser>();
            A.CallTo(() => user.GetAvatarUrl(ImageFormat.Auto, 2048)).Returns(AnAvatarURL);

            var result = (TaylorBotEmbedResult)await _discordInfoModule.AvatarAsync(user);

            result.Embed.Image.Value.Url.Should().Be(AnAvatarURL);
        }
    }
}
