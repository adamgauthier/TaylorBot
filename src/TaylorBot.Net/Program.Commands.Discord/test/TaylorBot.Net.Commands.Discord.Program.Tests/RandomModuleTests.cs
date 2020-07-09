using Discord;
using FakeItEasy;
using FluentAssertions;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules;
using TaylorBot.Net.Commands.Types;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests
{
    public class RandomModuleTests
    {
        private readonly IUser _commandUser = A.Fake<IUser>();
        private readonly ITaylorBotCommandContext _commandContext = A.Fake<ITaylorBotCommandContext>(o => o.Strict());
        private readonly RandomModule _randomModule;

        public RandomModuleTests()
        {
            _randomModule = new RandomModule();
            _randomModule.SetContext(_commandContext);
            A.CallTo(() => _commandContext.User).Returns(_commandUser);
        }

        [Fact]
        public async Task DiceAsync_ThenReturnsEmbedTitleWithFaceCount()
        {
            const int FaceCount = 6;

            var result = (TaylorBotEmbedResult)await _randomModule.DiceAsync(new PositiveInt32(FaceCount));

            result.Embed.Title.Should().Contain(FaceCount.ToString());
        }
    }
}
