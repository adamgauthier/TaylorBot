using Discord;
using FakeItEasy;
using FluentAssertions;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.Plus.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Plus.Domain;
using TaylorBot.Net.Commands.Discord.Program.Tests.Helpers;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests
{
    public class PlusModuleTests
    {
        private readonly IUser _commandUser = A.Fake<IUser>();
        private readonly ITaylorBotCommandContext _commandContext = A.Fake<ITaylorBotCommandContext>();
        private readonly IPlusRepository _plusRepository = A.Fake<IPlusRepository>(o => o.Strict());
        private readonly IPlusUserRepository _plusUserRepository = A.Fake<IPlusUserRepository>(o => o.Strict());
        private readonly PlusModule _plusModule;

        public PlusModuleTests()
        {
            _plusModule = new PlusModule(new SimpleCommandRunner(), _plusRepository, _plusUserRepository);
            _plusModule.SetContext(_commandContext);
            A.CallTo(() => _commandContext.User).Returns(_commandUser);
            A.CallTo(() => _commandContext.CommandPrefix).Returns("!");
        }

        [Fact]
        public async Task PlusAsync_WhenNonPlusUser_ThenReturnsSuccessEmbed()
        {
            A.CallTo(() => _commandContext.Guild).Returns(null!);
            A.CallTo(() => _plusUserRepository.GetPlusUserAsync(_commandUser)).Returns(null);

            var result = (await _plusModule.PlusAsync()).GetResult<EmbedResult>();

            result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
        }

        [Fact]
        public async Task AddAsync_WhenMaxGuilds_ThenReturnsErrorEmbed()
        {
            A.CallTo(() => _commandContext.Guild).Returns(A.Fake<IGuild>(o => o.Strict()));
            A.CallTo(() => _plusUserRepository.GetPlusUserAsync(_commandUser)).Returns(new PlusUser(IsActive: true, MaxPlusGuilds: 2, new[] { "A Server", "Another Server" }));

            var result = (await _plusModule.AddAsync()).GetResult<EmbedResult>();

            result.Embed.Color.Should().Be(TaylorBotColors.ErrorColor);
        }

        [Fact]
        public async Task AddAsync_WhenUnderMaxGuilds_ThenReturnsDiamondEmbed()
        {
            A.CallTo(() => _commandContext.Guild).Returns(A.Fake<IGuild>());
            A.CallTo(() => _plusUserRepository.GetPlusUserAsync(_commandUser)).Returns(new PlusUser(IsActive: true, MaxPlusGuilds: 2, new[] { "A Server" }));
            A.CallTo(() => _plusUserRepository.AddPlusGuildAsync(_commandUser, _commandContext.Guild)).Returns(new ValueTask());

            var result = (await _plusModule.AddAsync()).GetResult<EmbedResult>();

            result.Embed.Color.Should().Be(TaylorBotColors.DiamondBlueColor);
        }

        [Fact]
        public async Task RemoveAsync_ThenReturnsSuccessEmbed()
        {
            A.CallTo(() => _commandContext.Guild).Returns(A.Fake<IGuild>());
            A.CallTo(() => _plusUserRepository.DisablePlusGuildAsync(_commandUser, _commandContext.Guild)).Returns(new ValueTask());

            var result = (await _plusModule.RemoveAsync()).GetResult<EmbedResult>();

            result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
        }
    }
}
