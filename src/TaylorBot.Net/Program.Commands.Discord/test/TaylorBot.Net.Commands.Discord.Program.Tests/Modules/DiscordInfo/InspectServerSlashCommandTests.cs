using Discord;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using TaylorBot.Net.Commands.Discord.Program.Modules.DiscordInfo.Commands;
using TaylorBot.Net.Commands.Discord.Program.Tests.Helpers;
using TaylorBot.Net.Core.Snowflake;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests.Modules.DiscordInfo;

public class InspectServerSlashCommandTests
{
    private readonly RunContext _runContext;

    private readonly InspectServerSlashCommand _command;

    public InspectServerSlashCommandTests()
    {
        var services = new ServiceCollection();
        services.AddSingleton(CommandUtils.Mentioner);

        _command = new(new(services.BuildServiceProvider()));
        _runContext = CommandUtils.CreateTestContext(_command);
    }

    [Fact]
    public async Task InspectServer_ThenReturnsIdFieldEmbed()
    {
        var guild = _runContext.Guild?.Fetched;
        ArgumentNullException.ThrowIfNull(guild);

        SnowflakeId AnId = 1;
        A.CallTo(() => guild.Id).Returns(AnId);
        A.CallTo(() => guild.VoiceRegionId).Returns("us-east");
        var role = A.Fake<IRole>();
        A.CallTo(() => role.Mention).Returns("<@0>");
        A.CallTo(() => guild.Roles).Returns([role]);

        var result = (EmbedResult)await (await _command.GetCommandAsync(_runContext, new())).RunAsync();

        result.Embed.Fields.Single(f => f.Name == "Id").Value.Should().Contain(guild.Id.ToString());
    }
}
