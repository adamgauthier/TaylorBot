using FluentAssertions;
using TaylorBot.Net.Commands.Discord.Program.Modules.DiscordInfo.Commands;
using TaylorBot.Net.Commands.Discord.Program.Tests.Helpers;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests;

public class InspectUserSlashCommandTests
{
    private readonly RunContext _runContext;
    private readonly InspectUserSlashCommand _command;

    public InspectUserSlashCommandTests()
    {
        _command = new();
        _runContext = CommandUtils.CreateTestContext(_command);
    }

    [Fact]
    public async Task InspectUser_ThenReturnsIdFieldEmbed()
    {
        const ulong AnId = 1;

        var user = _runContext.User with { Id = AnId };

        var result = (EmbedResult)await (await _command.GetCommandAsync(_runContext, new(new(user)))).RunAsync();

        result.Embed.Fields.Single(f => f.Name == "User Id").Value.Should().Contain($"{AnId}");
    }
}
