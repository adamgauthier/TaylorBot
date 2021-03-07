using Discord;
using FakeItEasy;
using FluentAssertions;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Logs.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Colors;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests
{
    public class LogModuleTests
    {
        public class DeletedModuleTests
        {
            private readonly IUser _commandUser = A.Fake<IUser>();
            private readonly IGuild _commandGuild = A.Fake<IGuild>();
            private readonly ITaylorBotCommandContext _commandContext = A.Fake<ITaylorBotCommandContext>(o => o.Strict());
            private readonly IDeletedLogChannelRepository _deletedLogChannelRepository = A.Fake<IDeletedLogChannelRepository>(o => o.Strict());
            private readonly LogModule.DeletedModule _deletedModule;

            public DeletedModuleTests()
            {
                _deletedModule = new LogModule.DeletedModule(_deletedLogChannelRepository);
                _deletedModule.SetContext(_commandContext);
                A.CallTo(() => _commandContext.User).Returns(_commandUser);
                A.CallTo(() => _commandContext.Guild).Returns(_commandGuild);
                A.CallTo(() => _commandContext.CommandPrefix).Returns("!");
            }

            [Fact]
            public async Task AddAsync_ThenReturnsSuccessEmbed()
            {
                var channel = A.Fake<ITextChannel>();
                A.CallTo(() => _deletedLogChannelRepository.AddOrUpdateDeletedLogAsync(channel)).Returns(new ValueTask());

                var result = (TaylorBotEmbedResult)await _deletedModule.AddAsync(new ChannelArgument<ITextChannel>(channel));

                result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
            }

            [Fact]
            public async Task RemoveAsync_ThenReturnsSuccessEmbed()
            {
                A.CallTo(() => _deletedLogChannelRepository.RemoveDeletedLogAsync(_commandGuild)).Returns(new ValueTask());

                var result = (TaylorBotEmbedResult)await _deletedModule.RemoveAsync();

                result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
            }
        }

        public class MemberModuleTests
        {
            private readonly IUser _commandUser = A.Fake<IUser>();
            private readonly IGuild _commandGuild = A.Fake<IGuild>();
            private readonly ITaylorBotCommandContext _commandContext = A.Fake<ITaylorBotCommandContext>(o => o.Strict());
            private readonly IMemberLogChannelRepository _memberLogChannelRepository = A.Fake<IMemberLogChannelRepository>(o => o.Strict());
            private readonly LogModule.MemberModule _memberModule;

            public MemberModuleTests()
            {
                _memberModule = new LogModule.MemberModule(_memberLogChannelRepository);
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

                var result = (TaylorBotEmbedResult)await _memberModule.AddAsync(new ChannelArgument<ITextChannel>(channel));

                result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
            }

            [Fact]
            public async Task RemoveAsync_ThenReturnsSuccessEmbed()
            {
                A.CallTo(() => _memberLogChannelRepository.RemoveMemberLogAsync(_commandGuild)).Returns(new ValueTask());

                var result = (TaylorBotEmbedResult)await _memberModule.RemoveAsync();

                result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
            }
        }
    }
}
