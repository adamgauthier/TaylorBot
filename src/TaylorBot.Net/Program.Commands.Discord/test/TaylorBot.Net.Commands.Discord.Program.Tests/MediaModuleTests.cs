using Discord;
using FakeItEasy;
using FluentAssertions;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.Image.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Image.Domain;
using TaylorBot.Net.Commands.Discord.Program.Tests.Helpers;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests
{
    public class MediaModuleTests
    {
        private readonly IUser _commandUser = A.Fake<IUser>();
        private readonly IMessageChannel _channel = A.Fake<IMessageChannel>();
        private readonly ITaylorBotCommandContext _commandContext = A.Fake<ITaylorBotCommandContext>();
        private readonly IPlusRepository _plusRepository = A.Fake<IPlusRepository>(o => o.Strict());
        private readonly IRateLimiter _rateLimiter = A.Fake<IRateLimiter>(o => o.Strict());
        private readonly IImageSearchClient _imageSearchClient = A.Fake<IImageSearchClient>(o => o.Strict());
        private readonly MediaModule _mediaModule;

        public MediaModuleTests()
        {
            _mediaModule = new MediaModule(new SimpleCommandRunner(), _plusRepository, _rateLimiter, _imageSearchClient);
            _mediaModule.SetContext(_commandContext);
            A.CallTo(() => _commandContext.User).Returns(_commandUser);
            A.CallTo(() => _commandContext.Channel).Returns(_channel);
        }

        [Fact]
        public async Task ImageAsync_WhenDailyLimitExceeded_ThenReturnsErrorEmbed()
        {
            const string Text = "taylor swift";
            A.CallTo(() => _rateLimiter.VerifyDailyLimitAsync(_commandUser, A<string>.Ignored)).Returns(null);
            A.CallTo(() => _imageSearchClient.SearchImagesAsync(Text)).Returns(new DailyLimitExceeded());

            var result = (await _mediaModule.ImageAsync(Text)).GetResult<EmbedResult>();

            result.Embed.Color.Should().Be(TaylorBotColors.ErrorColor);
        }

        [Fact]
        public async Task ImageAsync_ThenReturnsSuccessEmbed()
        {
            const string Text = "taylor swift";
            A.CallTo(() => _rateLimiter.VerifyDailyLimitAsync(_commandUser, A<string>.Ignored)).Returns(null);
            A.CallTo(() => _imageSearchClient.SearchImagesAsync(Text)).Returns(new SuccessfulSearch(
                Images: new ImageResult[] {
                    new(
                        Title: "Taylor Swift - Wikipedia",
                        PageUrl: new("https://en.wikipedia.org/wiki/Taylor_Swift"),
                        ImageUrl: new("https://upload.wikimedia.org/wikipedia/commons/b/b5/191125_Taylor_Swift_at_the_2019_American_Music_Awards_%28cropped%29.png")
                    )
                },
                ResultCount: "1",
                SearchTimeSeconds: "5"
            ));

            var result = (await _mediaModule.ImageAsync(Text)).GetResult<PageMessageResult>();
            await result.PageMessage.SendAsync(_commandUser, _channel);

            A.CallTo(() => _channel.SendMessageAsync(
                null,
                false,
                A<Embed>.That.Matches(e => e.Color == TaylorBotColors.SuccessColor),
                null,
                null,
                null,
                null
            )).MustHaveHappenedOnceExactly();
        }
    }
}
