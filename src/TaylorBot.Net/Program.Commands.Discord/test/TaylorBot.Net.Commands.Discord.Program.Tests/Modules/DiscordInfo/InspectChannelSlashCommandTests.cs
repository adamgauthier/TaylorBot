using Discord;
using FakeItEasy;
using FluentAssertions;
using TaylorBot.Net.Commands.Discord.Program.Modules.DiscordInfo.Commands;
using TaylorBot.Net.Commands.Discord.Program.Tests.Helpers;
using TaylorBot.Net.Commands.Parsers.Channels;
using TaylorBot.Net.Core.Client;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests.Modules.DiscordInfo;

public class InspectChannelSlashCommandTests
{
    private readonly RunContext _runContext;
    private readonly ITaylorBotClient _taylorBotClient;

    private readonly InspectChannelSlashCommand _command;

    public InspectChannelSlashCommandTests()
    {
        _taylorBotClient = A.Fake<ITaylorBotClient>(o => o.Strict());

        _command = new(new(() => _taylorBotClient), new());
        _runContext = CommandUtils.CreateTestContext(_command);
    }

    [Fact]
    public async Task InspectChannel_ThenReturnsIdFieldEmbed()
    {
        const ulong AnId = 1;

        DiscordChannel channel = new(AnId, ChannelType.Voice);
        A.CallTo(() => _taylorBotClient.ResolveRequiredChannelAsync(AnId)).Returns(A.Fake<IChannel>());

        var result = (EmbedResult)await (await _command.GetCommandAsync(_runContext, new(new(channel)))).RunAsync();

        result.Embed.Fields.Single(f => f.Name == "Id").Value.Should().Contain(AnId.ToString());
    }
}
