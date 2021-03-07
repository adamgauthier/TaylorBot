using Discord;
using FakeItEasy;
using FluentAssertions;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules;
using TaylorBot.Net.Commands.Discord.Program.Plus.Domain;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests
{
    public class PlusModuleTests
    {
        private readonly IUser _commandUser = A.Fake<IUser>();
        private readonly ITaylorBotCommandContext _commandContext = A.Fake<ITaylorBotCommandContext>(o => o.Strict());
        private readonly IPlusRepository _plusRepository = A.Fake<IPlusRepository>(o => o.Strict());
        private readonly IPlusUserRepository _plusUserRepository = A.Fake<IPlusUserRepository>(o => o.Strict());
        private readonly PlusModule _plusModule;

        public PlusModuleTests()
        {
            _plusModule = new PlusModule(_plusRepository, _plusUserRepository);
            _plusModule.SetContext(_commandContext);
            A.CallTo(() => _commandContext.User).Returns(_commandUser);
            A.CallTo(() => _commandContext.CommandPrefix).Returns("!");
        }

        [Fact]
        public async Task PlusAsync_WhenNonPlusUser_ThenReturnsSuccessEmbed()
        {
            A.CallTo(() => _commandContext.Guild).Returns(null!);
            A.CallTo(() => _plusUserRepository.GetPlusUserAsync(_commandUser)).Returns(null);

            var result = (TaylorBotEmbedResult)await _plusModule.PlusAsync();

            result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
        }

        [Fact]
        public async Task AddAsync_WhenMaxGuilds_ThenReturnsErrorEmbed()
        {
            A.CallTo(() => _commandContext.Guild).Returns(A.Fake<IGuild>(o => o.Strict()));
            A.CallTo(() => _plusUserRepository.GetPlusUserAsync(_commandUser)).Returns(new PlusUser(IsActive: true, MaxPlusGuilds: 2, new[] { "A Server", "Another Server" }));

            var result = (TaylorBotEmbedResult)await _plusModule.AddAsync();

            result.Embed.Color.Should().Be(TaylorBotColors.ErrorColor);
        }

        [Fact]
        public async Task AddAsync_WhenUnderMaxGuilds_ThenReturnsDiamondEmbed()
        {
            A.CallTo(() => _commandContext.Guild).Returns(A.Fake<IGuild>());
            A.CallTo(() => _plusUserRepository.GetPlusUserAsync(_commandUser)).Returns(new PlusUser(IsActive: true, MaxPlusGuilds: 2, new[] { "A Server" }));
            A.CallTo(() => _plusUserRepository.AddPlusGuildAsync(_commandUser, _commandContext.Guild)).Returns(new ValueTask());

            var result = (TaylorBotEmbedResult)await _plusModule.AddAsync();

            result.Embed.Color.Should().Be(TaylorBotColors.DiamondBlueColor);
        }

        [Fact]
        public async Task RemoveAsync_ThenReturnsSuccessEmbed()
        {
            A.CallTo(() => _commandContext.Guild).Returns(A.Fake<IGuild>());
            A.CallTo(() => _plusUserRepository.DisablePlusGuildAsync(_commandUser, _commandContext.Guild)).Returns(new ValueTask());

            var result = (TaylorBotEmbedResult)await _plusModule.RemoveAsync();

            result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
        }
    }
}
