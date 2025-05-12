using Discord;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Commands.Discord.Program.Modules.Image.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Image.Domain;
using TaylorBot.Net.Commands.Discord.Program.Tests.Helpers;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests.Modules.Image;

public class ImageSlashCommandTests : IAsyncDisposable
{
    private readonly IUserMessage _message = A.Fake<IUserMessage>();
    private readonly IMessageChannel _channel = A.Fake<ITextChannel>();
    private readonly IImageSearchClient _imageSearchClient = A.Fake<IImageSearchClient>(o => o.Strict());
    private readonly ImageSlashCommand _command;
    private readonly IMemoryCache _memoryCache;
    private readonly RunContext _runContext;

    public ImageSlashCommandTests()
    {
        ServiceCollection services = new();
        services.AddSingleton(CommandUtils.Mentioner);
        services.AddSingleton(A.Fake<IPlusRepository>(o => o.Strict()));

        _memoryCache = new MemoryCache(A.Fake<IOptions<MemoryCacheOptions>>());
        _command = new ImageSlashCommand(
            CommandUtils.UnlimitedRateLimiter,
            _imageSearchClient,
            new(services.BuildServiceProvider()),
            new(new(_memoryCache))
        );
        _runContext = CommandUtils.CreateTestContext(_command);

        A.CallTo(() => _message.Channel).Returns(_channel);
    }

    public ValueTask DisposeAsync()
    {
        _memoryCache.Dispose();
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    [Fact]
    public async Task GetCommandAsync_WhenDailyLimitExceeded_ThenReturnsErrorEmbed()
    {
        const string Text = "taylor swift";
        A.CallTo(() => _imageSearchClient.SearchImagesAsync(Text)).Returns(new DailyLimitExceeded());

        var command = await _command.GetCommandAsync(_runContext, new(new ParsedString(Text)));
        var result = (await command.RunAsync()).Should().BeOfType<EmbedResult>().Which;

        result.Embed.Color.Should().Be(TaylorBotColors.ErrorColor);
    }

    [Fact]
    public async Task GetCommandAsync_ThenReturnsSuccessEmbed()
    {
        const string Text = "taylor swift";
        A.CallTo(() => _imageSearchClient.SearchImagesAsync(Text)).Returns(new SuccessfulSearch(
            Images: [
                new(
                    Title: "Taylor Swift - Wikipedia",
                    PageUrl: new("https://en.wikipedia.org/wiki/Taylor_Swift"),
                    ImageUrl: new("https://upload.wikimedia.org/wikipedia/commons/b/b5/191125_Taylor_Swift_at_the_2019_American_Music_Awards_%28cropped%29.png")
                )
            ],
            ResultCount: "1",
            SearchTimeSeconds: "5"
        ));

        var command = await _command.GetCommandAsync(_runContext, new(new ParsedString(Text)));
        var result = (await command.RunAsync()).Should().BeOfType<MessageResult>().Which;

        result.Message.Content.Embeds.Should().ContainSingle().Which
            .Color.Should().Be(TaylorBotColors.SuccessColor);
    }

    [Fact]
    public async Task GetCommandAsync_WhenGenericError_ThenReturnsErrorEmbed()
    {
        const string Text = "taylor swift";
        A.CallTo(() => _imageSearchClient.SearchImagesAsync(Text)).Returns(new GenericError(new NotImplementedException()));

        var command = await _command.GetCommandAsync(_runContext, new(new ParsedString(Text)));
        var result = (await command.RunAsync()).Should().BeOfType<EmbedResult>().Which;

        result.Embed.Color.Should().Be(TaylorBotColors.ErrorColor);
    }
}
