using Discord;
using FakeItEasy;
using FluentAssertions;
using System.Reflection;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules;
using TaylorBot.Net.Commands.Discord.Program.Services;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests
{
    public class DiscordInfoModuleTests
    {
        private readonly PresenceFactory _presenceFactory = new PresenceFactory();

        private readonly ITaylorBotCommandContext _commandContext = A.Fake<ITaylorBotCommandContext>(o => o.Strict());
        private readonly UserStatusStringMapper _userStatusStringMapper = new UserStatusStringMapper();
        private readonly DiscordInfoModule _discordInfoModule;

        public DiscordInfoModuleTests()
        {
            _discordInfoModule = new DiscordInfoModule(_userStatusStringMapper);
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

        [Fact]
        public async Task StatusAsync_WhenNoActivity_ThenReturnsOnlineStatusEmbed()
        {
            const UserStatus AUserStatus = UserStatus.DoNotDisturb;
            var user = A.Fake<IUser>();
            A.CallTo(() => user.Activity).Returns(null);
            A.CallTo(() => user.Status).Returns(AUserStatus);

            var result = (TaylorBotEmbedResult)await _discordInfoModule.StatusAsync(user);

            result.Embed.Description.Should().Be(_userStatusStringMapper.MapStatusToString(AUserStatus));
        }

        [Fact]
        public async Task StatusAsync_WhenCustomStatusActivity_ThenReturnsCustomStatusEmbed()
        {
            const string ACustomStatus = "the end of a decade but the start of an age";
            var user = A.Fake<IUser>();
            A.CallTo(() => user.Activity).Returns(_presenceFactory.CreateCustomStatus(ACustomStatus));

            var result = (TaylorBotEmbedResult)await _discordInfoModule.StatusAsync(user);

            result.Embed.Description.Should().Contain(ACustomStatus);
        }
    }
}
