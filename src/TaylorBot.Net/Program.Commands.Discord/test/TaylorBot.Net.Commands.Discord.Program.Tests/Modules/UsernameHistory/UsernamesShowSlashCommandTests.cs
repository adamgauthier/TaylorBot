using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Commands.Discord.Program.Modules.UsernameHistory.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.UsernameHistory.Domain;
using TaylorBot.Net.Commands.Discord.Program.Tests.Helpers;
using TaylorBot.Net.Core.Colors;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests.Modules.UsernameHistory;

public class UsernamesShowSlashCommandTests : IAsyncDisposable
{
    private readonly IUsernameHistoryRepository _usernameHistoryRepository = A.Fake<IUsernameHistoryRepository>(o => o.Strict());
    private readonly RunContext _runContext;
    private readonly UsernamesShowSlashCommand _command;
    private readonly IMemoryCache _memoryCache;

    public UsernamesShowSlashCommandTests()
    {
        _memoryCache = new MemoryCache(A.Fake<IOptions<MemoryCacheOptions>>());
        _command = new UsernamesShowSlashCommand(_usernameHistoryRepository, CommandUtils.Mentioner, new(new(_memoryCache)));
        _runContext = CommandUtils.CreateTestContext(_command);
    }

    public ValueTask DisposeAsync()
    {
        _memoryCache.Dispose();
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    [Fact]
    public async Task GetAsync_WhenUsernameHistoryPrivate_ThenReturnsSuccessEmbed()
    {
        A.CallTo(() => _usernameHistoryRepository.IsUsernameHistoryHiddenFor(_runContext.User)).Returns(true);

        var result = (EmbedResult)await (await _command.GetCommandAsync(_runContext, new(new(_runContext.User)))).RunAsync();

        result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
        result.Embed.Description.Should().Contain("private");
    }

    [Fact]
    public async Task GetAsync_WhenUsernameHistoryPublic_ThenReturnsPageEmbedWithUsername()
    {
        const string AUsername = "Enchanted13";

        A.CallTo(() => _usernameHistoryRepository.IsUsernameHistoryHiddenFor(_runContext.User)).Returns(false);
        A.CallTo(() => _usernameHistoryRepository.GetUsernameHistoryFor(_runContext.User, 75)).Returns([
            new UsernameChange(Username: AUsername, ChangedAt: DateTimeOffset.UtcNow.AddDays(-1))
        ]);

        var result = (MessageResult)await (await _command.GetCommandAsync(_runContext, new(new(_runContext.User)))).RunAsync();

        result.Message.Content.Embeds.Should().ContainSingle().Which
            .Description.Should().Contain(AUsername);
    }
}
