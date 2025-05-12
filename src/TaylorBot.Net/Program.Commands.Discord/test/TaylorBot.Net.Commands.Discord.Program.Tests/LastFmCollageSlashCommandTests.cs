using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
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

public class LastFmCollageSlashCommandTests : IAsyncDisposable
{
    private readonly DiscordUser _commandUser = CommandUtils.AUser;

    private readonly ILogger<LastFmCollageSlashCommand> _logger = A.Fake<ILogger<LastFmCollageSlashCommand>>();
    private readonly IOptionsMonitor<LastFmOptions> _options = A.Fake<IOptionsMonitor<LastFmOptions>>(o => o.Strict());
    private readonly ILastFmUsernameRepository _lastFmUsernameRepository = A.Fake<ILastFmUsernameRepository>(o => o.Strict());
    private readonly LastFmPeriodStringMapper _lastFmPeriodStringMapper = new();
    private readonly IHttpClientFactory _clientFactory = A.Fake<IHttpClientFactory>(o => o.Strict());

    private readonly AlwaysSucceedHttpMessageHandler _handler = new();
    private readonly HttpClient _client;

    private readonly LastFmCollageSlashCommand _lastFmCollageSlashCommand;

    public LastFmCollageSlashCommandTests()
    {
        _client = new(_handler);
        A.CallTo(() => _clientFactory.CreateClient(Microsoft.Extensions.Options.Options.DefaultName)).Returns(_client);
        _lastFmCollageSlashCommand = new(_logger, _lastFmUsernameRepository, new(_lastFmPeriodStringMapper), _lastFmPeriodStringMapper, _clientFactory);
    }

    public ValueTask DisposeAsync()
    {
        _handler.Dispose();
        _client.Dispose();
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
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
        LastFmUsername lastFmUsername = new("taylorswift");
        A.CallTo(() => _options.CurrentValue).Returns(new LastFmOptions { LastFmEmbedFooterIconUrl = "https://last.fm./icon.png" });
        A.CallTo(() => _lastFmUsernameRepository.GetLastFmUsernameAsync(_commandUser)).Returns(lastFmUsername);

        var command = await _lastFmCollageSlashCommand.GetCommandAsync(null!, new(null, new(null), new(_commandUser)));
        var result = (MessageResult)await command.RunAsync();

        result.Message.Content.Embeds[0].Image.Should().NotBeNull();
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
