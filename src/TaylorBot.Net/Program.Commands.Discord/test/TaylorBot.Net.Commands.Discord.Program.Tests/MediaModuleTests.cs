using Discord;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Commands.Discord.Program.Modules.Image.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Image.Domain;
using TaylorBot.Net.Commands.Discord.Program.Tests.Helpers;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests;

public class MediaModuleTests : IAsyncDisposable
{
    private readonly IUser _commandUser = A.Fake<IUser>();
    private readonly IUserMessage _message = A.Fake<IUserMessage>();
    private readonly IMessageChannel _channel = A.Fake<ITextChannel>();
    private readonly ITaylorBotCommandContext _commandContext = A.Fake<ITaylorBotCommandContext>();
    private readonly IImageSearchClient _imageSearchClient = A.Fake<IImageSearchClient>(o => o.Strict());
    private readonly MediaModule _mediaModule;
    private readonly IMemoryCache _memoryCache;

    public MediaModuleTests()
    {
        ServiceCollection services = new();
        services.AddSingleton(CommandUtils.Mentioner);
        services.AddSingleton(A.Fake<IPlusRepository>(o => o.Strict()));

        _memoryCache = new MemoryCache(A.Fake<IOptions<MemoryCacheOptions>>());
        _mediaModule = new MediaModule(new SimpleCommandRunner(), new ImageSlashCommand(CommandUtils.UnlimitedRateLimiter, _imageSearchClient, new(services.BuildServiceProvider()), new(new(_memoryCache))));
        _mediaModule.SetContext(_commandContext);
        A.CallTo(() => _commandContext.Channel).Returns(_channel);
        A.CallTo(() => _commandContext.User).Returns(_commandUser);
        A.CallTo(() => _message.Channel).Returns(_channel);
    }

    public ValueTask DisposeAsync()
    {
        _memoryCache.Dispose();
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    [Fact]
    public async Task ImageAsync_WhenDailyLimitExceeded_ThenReturnsErrorEmbed()
    {
        const string Text = "taylor swift";
        A.CallTo(() => _imageSearchClient.SearchImagesAsync(Text)).Returns(new DailyLimitExceeded());

        var result = (await _mediaModule.ImageAsync(Text)).GetResult<EmbedResult>();

        result.Embed.Color.Should().Be(TaylorBotColors.ErrorColor);
    }

    [Fact]
    public async Task ImageAsync_ThenReturnsSuccessEmbed()
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

        var result = (await _mediaModule.ImageAsync(Text)).GetResult<PageMessageResult>();
        using var _ = await result.PageMessage.SendAsync(_commandUser, _message);

        A.CallTo(() => _channel.SendMessageAsync(
            null,
            false,
            A<Embed>.That.Matches(e => e.Color == TaylorBotColors.SuccessColor),
            null,
            A<AllowedMentions>.Ignored,
            A<MessageReference>.Ignored,
            null,
            null,
            null,
            MessageFlags.None,
            null
        )).MustHaveHappenedOnceExactly();
    }
}
