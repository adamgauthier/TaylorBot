using FluentAssertions;
using TaylorBot.Net.Commands.Discord.Program.Modules.DiscordInfo.Commands;
using TaylorBot.Net.Commands.Discord.Program.Tests.Helpers;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Snowflake;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests.Modules.DiscordInfo;

public class InspectRoleSlashCommandTests
{
    private readonly RunContext _runContext;

    private readonly InspectRoleSlashCommand _command;

    public InspectRoleSlashCommandTests()
    {
        _command = new();
        _runContext = CommandUtils.CreateTestContext(_command);
    }

    [Fact]
    public async Task InspectRole_ThenReturnsIdFieldEmbed()
    {
        SnowflakeId AnId = new(1);

        DiscordRole role = new(AnId, string.Empty, 0, false, null, null, 0, string.Empty, false, false, null, 0);

        var result = (EmbedResult)await (await _command.GetCommandAsync(_runContext, new(new(role)))).RunAsync();

        result.Embed.Fields.Single(f => f.Name == "Id").Value.Should().Contain(AnId);
    }
}
