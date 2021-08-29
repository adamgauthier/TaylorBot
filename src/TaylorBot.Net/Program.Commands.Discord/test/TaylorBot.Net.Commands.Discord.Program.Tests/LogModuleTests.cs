using Discord;
using FakeItEasy;
using FluentAssertions;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.Logs.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Logs.Domain;
using TaylorBot.Net.Commands.Discord.Program.Tests.Helpers;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Colors;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests
{
    public class LogModuleTests
    {
        public class MemberModuleTests
        {
            private readonly IUser _commandUser = A.Fake<IUser>();
            private readonly IGuild _commandGuild = A.Fake<IGuild>();
            private readonly ITaylorBotCommandContext _commandContext = A.Fake<ITaylorBotCommandContext>();
            private readonly IPlusRepository _plusRepository = A.Fake<IPlusRepository>(o => o.Strict());
            private readonly IMemberLogChannelRepository _memberLogChannelRepository = A.Fake<IMemberLogChannelRepository>(o => o.Strict());
            private readonly LogModule.MemberModule _memberModule;

            public MemberModuleTests()
            {
                _memberModule = new LogModule.MemberModule(new SimpleCommandRunner(), _plusRepository, _memberLogChannelRepository);
                _memberModule.SetContext(_commandContext);
                A.CallTo(() => _commandContext.User).Returns(_commandUser);
                A.CallTo(() => _commandContext.Guild).Returns(_commandGuild);
                A.CallTo(() => _commandContext.CommandPrefix).Returns("!");
            }

            [Fact]
            public async Task AddAsync_ThenReturnsSuccessEmbed()
            {
                var channel = A.Fake<ITextChannel>();
                A.CallTo(() => _memberLogChannelRepository.AddOrUpdateMemberLogAsync(channel)).Returns(new ValueTask());

                var result = (await _memberModule.AddAsync(new ChannelArgument<ITextChannel>(channel))).GetResult<EmbedResult>();

                result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
            }

            [Fact]
            public async Task RemoveAsync_ThenReturnsSuccessEmbed()
            {
                A.CallTo(() => _memberLogChannelRepository.RemoveMemberLogAsync(_commandGuild)).Returns(new ValueTask());

                var result = (await _memberModule.RemoveAsync()).GetResult<EmbedResult>();

                result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
            }
        }
    }
}
