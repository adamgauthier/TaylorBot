using Discord;
using FakeItEasy;
using FluentAssertions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace TaylorBot.Net.Commands.Types.Tests
{
    public class MentionedUserTypeReaderTests
    {
        private const ulong AnId = 1;
        private static readonly IUser AUser = A.Fake<IUser>();
        private static readonly IGuildUser AGuildUser = A.Fake<IGuildUser>();

        private readonly ITaylorBotCommandContext _commandContext = A.Fake<ITaylorBotCommandContext>(o => o.Strict());
        private readonly IServiceProvider _serviceProvider = A.Fake<IServiceProvider>();

        [Fact]
        public async Task ReadAsync_WhenIUserMentionInChannel_ThenReturnsUser()
        {
            var mentionedUserTypeReader = new MentionedUserTypeReader<IUser>();
            var channel = A.Fake<IMessageChannel>(o => o.Strict());
            A.CallTo(() => _commandContext.Guild).Returns(null);
            A.CallTo(() => _commandContext.Channel).Returns(channel);
            A.CallTo(() => channel.GetUserAsync(AnId, CacheMode.AllowDownload, null)).Returns(AUser);

            var result = (MentionedUser<IUser>)(await mentionedUserTypeReader.ReadAsync(_commandContext, MentionUtils.MentionUser(AnId), _serviceProvider)).Values.Single().Value;

            result.User.Should().Be(AUser);
        }

        [Fact]
        public async Task ReadAsync_WhenIGuildUserMentionInGuild_ThenReturnsIGuildUser()
        {
            var mentionedUserTypeReader = new MentionedUserTypeReader<IGuildUser>();
            var guild = A.Fake<IGuild>(o => o.Strict());
            A.CallTo(() => _commandContext.Guild).Returns(guild);
            A.CallTo(() => guild.GetUserAsync(AnId, CacheMode.AllowDownload, null)).Returns(AGuildUser);

            var result = (MentionedUser<IGuildUser>)(await mentionedUserTypeReader.ReadAsync(_commandContext, MentionUtils.MentionUser(AnId), _serviceProvider)).Values.Single().Value;

            result.User.Should().Be(AGuildUser);
        }
    }
}
