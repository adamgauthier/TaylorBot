using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Domain;
using TaylorBot.Net.Commands.Discord.Program.Options;
using TaylorBot.Net.Commands.Discord.Program.Tests.Helpers;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.User;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests;

public class LastFmCollageSlashCommandTests
{
    private readonly DiscordUser _commandUser = CommandUtils.AUser;
    private readonly IOptionsMonitor<LastFmOptions> _options = A.Fake<IOptionsMonitor<LastFmOptions>>(o => o.Strict());
    private readonly ILastFmUsernameRepository _lastFmUsernameRepository = A.Fake<ILastFmUsernameRepository>(o => o.Strict());
    private readonly LastFmPeriodStringMapper _lastFmPeriodStringMapper = new();
    private readonly LastFmCollageSlashCommand _lastFmCollageSlashCommand;

    public LastFmCollageSlashCommandTests()
    {
        _lastFmCollageSlashCommand = new(_lastFmUsernameRepository, new(_lastFmPeriodStringMapper), _lastFmPeriodStringMapper, new HttpClient(new AlwaysSucceedHttpMessageHandler()));
    }

    [Fact]
    public async Task CollageAsync_WhenUsernameNotSet_ThenReturnsErrorEmbed()
    {
        A.CallTo(() => _lastFmUsernameRepository.GetLastFmUsernameAsync(_commandUser)).Returns(null);

        var command = await _lastFmCollageSlashCommand.GetCommandAsync(null!, new(null, new(null), new(_commandUser)));
        var result = (EmbedResult)await command.RunAsync();

        result.Embed.Color.Should().Be(TaylorBotColors.ErrorColor);
    }

    [Fact]
    public async Task CollageAsync_ThenReturnsEmbedWithImage()
    {
        var lastFmUsername = new LastFmUsername("taylorswift");
        A.CallTo(() => _options.CurrentValue).Returns(new LastFmOptions { LastFmEmbedFooterIconUrl = "https://last.fm./icon.png" });
        A.CallTo(() => _lastFmUsernameRepository.GetLastFmUsernameAsync(_commandUser)).Returns(lastFmUsername);

        var command = await _lastFmCollageSlashCommand.GetCommandAsync(null!, new(null, new(null), new(_commandUser)));
        var result = (MessageResult)await command.RunAsync();

        result.Content.Embeds[0].Image.Should().NotBeNull();
    }

    public class AlwaysSucceedHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes("Some file content")))
            };

            return Task.FromResult(response);
        }
    }
}
