using Discord;
using FakeItEasy;
using FluentAssertions;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.Stats.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Stats.Domain;
using TaylorBot.Net.Commands.Discord.Program.Tests.Helpers;
using TaylorBot.Net.Commands.DiscordNet;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests;

public class StatsModuleTests
{
    private readonly IUser _commandUser = A.Fake<IUser>();
    private readonly IGuild _commandGuild = A.Fake<IGuild>();
    private readonly ITaylorBotCommandContext _commandContext = A.Fake<ITaylorBotCommandContext>();
    private readonly IBotInfoRepository _botInfoRepository = A.Fake<IBotInfoRepository>(o => o.Strict());
    private readonly StatsModule _statsModule;

    public StatsModuleTests()
    {
        _statsModule = new StatsModule(new SimpleCommandRunner(), _botInfoRepository);
        _statsModule.SetContext(_commandContext);
        A.CallTo(() => _commandContext.User).Returns(_commandUser);
        A.CallTo(() => _commandContext.Guild).Returns(_commandGuild);
    }

    [Fact]
    public async Task BotInfoAsync_ThenReturnsEmbedWithCorrectDescriptionAndFields()
    {
        const string Description = "An application";
        const string ProductVersion = "2.0.0";
        var client = A.Fake<IDiscordClient>(o => o.Strict());
        var clientUser = A.Fake<ISelfUser>();
        var owner = A.Fake<IUser>();
        var application = A.Fake<IApplication>(o => o.Strict());

        A.CallTo(() => _botInfoRepository.GetProductVersionAsync()).Returns(ProductVersion);
        A.CallTo(() => _commandContext.Client).Returns(client);
        A.CallTo(() => client.CurrentUser).Returns(clientUser);
        A.CallTo(() => client.GetApplicationInfoAsync(null)).Returns(application);
        A.CallTo(() => application.Description).Returns(Description);
        A.CallTo(() => application.Owner).Returns(owner);
        A.CallTo(() => owner.Mention).Returns("<@1>");

        var result = (await _statsModule.BotInfoAsync()).GetResult<EmbedResult>();

        result.Embed.Description.Should().Be(Description);
        result.Embed.Fields.Single(f => f.Name == "Version").Value.Should().Be(ProductVersion);
    }
}
