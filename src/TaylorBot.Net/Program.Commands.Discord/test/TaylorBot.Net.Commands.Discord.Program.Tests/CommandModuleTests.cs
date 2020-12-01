using Discord;
using FakeItEasy;
using FluentAssertions;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests
{
    public class CommandModuleTests
    {
        private readonly IUser _commandUser = A.Fake<IUser>();
        private readonly ITaylorBotCommandContext _commandContext = A.Fake<ITaylorBotCommandContext>(o => o.Strict());
        private readonly IDisabledCommandRepository _disabledCommandRepository = A.Fake<IDisabledCommandRepository>(o => o.Strict());
        private readonly CommandModule _commandModule;

        public CommandModuleTests()
        {
            _commandModule = new CommandModule(_disabledCommandRepository);
            _commandModule.SetContext(_commandContext);
            A.CallTo(() => _commandContext.User).Returns(_commandUser);
        }

        [Fact]
        public async Task EnableGlobalAsync_ThenReturnsSuccessEmbed()
        {
            const string CommandName = "avatar";
            A.CallTo(() => _disabledCommandRepository.EnableGloballyAsync(CommandName)).Returns(default);
            var command = new ICommandRepository.Command(name: CommandName, moduleName: string.Empty);

            var result = (TaylorBotEmbedResult)await _commandModule.EnableGlobalAsync(command);

            result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
        }

        [Fact]
        public async Task DisableGlobalAsync_WhenGuardedModule_ThenReturnsErrorEmbed()
        {
            const string CommandName = "avatar";
            const string DisabledMesssage = "The command is down for maintenance.";
            A.CallTo(() => _disabledCommandRepository.DisableGloballyAsync(CommandName, DisabledMesssage)).Returns(DisabledMesssage);
            var command = new ICommandRepository.Command(name: CommandName, moduleName: "Framework");

            var result = (TaylorBotEmbedResult)await _commandModule.DisableGlobalAsync(command, DisabledMesssage);

            result.Embed.Color.Should().Be(TaylorBotColors.ErrorColor);
        }

        [Fact]
        public async Task DisableGlobalAsync_ThenReturnsSuccessEmbed()
        {
            const string CommandName = "avatar";
            const string DisabledMesssage = "The command is down for maintenance.";
            A.CallTo(() => _disabledCommandRepository.DisableGloballyAsync(CommandName, DisabledMesssage)).Returns(DisabledMesssage);
            var command = new ICommandRepository.Command(name: CommandName, moduleName: string.Empty);

            var result = (TaylorBotEmbedResult)await _commandModule.DisableGlobalAsync(command, DisabledMesssage);

            result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
        }
    }
}
