using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using TaylorBot.Net.Commands.Discord.Program.Modules.Server.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Stats.Domain;
using TaylorBot.Net.Commands.Discord.Program.Tests.Helpers;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests;

public class ServerPopulationSlashCommandTests
{
    private readonly IServerStatsRepository _serverStatsRepository = A.Fake<IServerStatsRepository>(o => o.Strict());
    private readonly RunContext _runContext;
    private readonly ServerPopulationSlashCommand _command;

    public ServerPopulationSlashCommandTests()
    {
        ServiceCollection services = new();
        services.AddSingleton(CommandUtils.Mentioner);

        _command = new(_serverStatsRepository, new(services.BuildServiceProvider()));
        _runContext = CommandUtils.CreateTestContext(_command);
    }

    [Fact]
    public async Task ServerPopulation_WhenNoData_ThenReturnsEmbedWithNoDataAndNoPercent()
    {
        A.CallTo(() => _serverStatsRepository.GetAgeStatsInGuildAsync(_runContext.Guild!)).Returns(new AgeStats(AgeAverage: null, AgeMedian: null));
        A.CallTo(() => _serverStatsRepository.GetGenderStatsInGuildAsync(_runContext.Guild!)).Returns(new GenderStats(TotalCount: 0, MaleCount: 0, FemaleCount: 0, OtherCount: 0));

        var result = (EmbedResult)await (await _command.GetCommandAsync(_runContext, new())).RunAsync();

        result.Embed.Fields.Single(f => f.Name == "Age").Value.Should().Contain("No Data");
        result.Embed.Fields.Single(f => f.Name == "Gender").Value.Should().NotContain("%");
    }

    [Fact]
    public async Task ServerPopulation_ThenReturnsEmbedWithAgeStatsAndGenderPercent()
    {
        const decimal AgeAverage = 22;
        const decimal AgeMedian = 15;
        const long MaleCount = 5;
        const long FemaleCount = 3;
        const long OtherCount = 2;
        const long TotalCount = 10;

        A.CallTo(() => _serverStatsRepository.GetAgeStatsInGuildAsync(_runContext.Guild!)).Returns(new AgeStats(AgeAverage, AgeMedian));
        A.CallTo(() => _serverStatsRepository.GetGenderStatsInGuildAsync(_runContext.Guild!)).Returns(new GenderStats(
            TotalCount, MaleCount, FemaleCount, OtherCount
        ));

        var result = (EmbedResult)await (await _command.GetCommandAsync(_runContext, new())).RunAsync();

        result.Embed.Fields.Single(f => f.Name == "Age").Value.Should().Contain(AgeAverage.ToString()).And.Contain(AgeMedian.ToString());
        result.Embed.Fields.Single(f => f.Name == "Gender").Value.Should().Contain($"{FemaleCount} ({FemaleCount}0.0%)");
    }
}
