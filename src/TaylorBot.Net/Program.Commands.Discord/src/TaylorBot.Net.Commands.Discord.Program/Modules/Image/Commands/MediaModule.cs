using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.Image.Domain;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.DiscordNet.PageMessages;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Image.Commands
{
    [Name("Media 📷")]
    public class MediaModule : TaylorBotModule
    {
        private readonly ICommandRunner _commandRunner;
        private readonly IPlusRepository _plusRepository;
        private readonly IRateLimiter _rateLimiter;
        private readonly IImageSearchClient _imageSearchClient;

        public MediaModule(ICommandRunner commandRunner, IPlusRepository plusRepository, IRateLimiter rateLimiter, IImageSearchClient imageSearchClient)
        {
            _commandRunner = commandRunner;
            _plusRepository = plusRepository;
            _rateLimiter = rateLimiter;
            _imageSearchClient = imageSearchClient;
        }

        [Command("image")]
        [Alias("imagen")]
        [Summary("Searches images based on the search text provided.")]
        public async Task<RuntimeResult> ImageAsync(
            [Remainder]
            string text
        )
        {
            var command = new Command(
                DiscordNetContextMapper.MapToCommandMetadata(Context),
                async () =>
                {
                    var result = await _rateLimiter.VerifyDailyLimitAsync(Context.User, "custom-search", "Searching Images");
                    if (result != null)
                        return result;

                    var searchResult = await _imageSearchClient.SearchImagesAsync(text);

                    switch (searchResult)
                    {
                        case SuccessfulSearch search:
                            EmbedBuilder BuildBaseEmbed() =>
                                new EmbedBuilder()
                                .WithUserAsAuthor(Context.User)
                                .WithColor(TaylorBotColors.SuccessColor)
                                .WithFooter($"{search.ResultCount} results found in {search.SearchTimeSeconds} seconds");

                            return new PageMessageResult(new PageMessage(new(
                                new EmbedPageMessageRenderer(new CustomSearchImagePageEditor(search.Images), BuildBaseEmbed),
                                Cancellable: true
                            )));

                        case DailyLimitExceeded _:
                            return new EmbedResult(new EmbedBuilder()
                                .WithUserAsAuthor(Context.User)
                                .WithColor(TaylorBotColors.ErrorColor)
                                .WithDescription(string.Join('\n', new[] {
                                "TaylorBot has reached its daily query limit for the underlying image search service. 😢",
                                "You'll have to wait for the limit to reset. Try again tomorrow!"
                                }))
                            .Build());

                        case GenericError error:
                            return new EmbedResult(new EmbedBuilder()
                                .WithUserAsAuthor(Context.User)
                                .WithColor(TaylorBotColors.ErrorColor)
                                .WithDescription(string.Join('\n', new[] {
                                "The underlying image search service returned an unexpected error. 😢",
                                "The site might be down. Try again later!"
                                }))
                            .Build());

                        default: throw new InvalidOperationException(searchResult.GetType().Name);
                    }
                },
                Preconditions: new[] { new PlusPrecondition(_plusRepository, PlusRequirement.PlusUserOrGuild) }
            );

            var context = DiscordNetContextMapper.MapToRunContext(Context);
            var result = await _commandRunner.RunAsync(command, context);

            return new TaylorBotResult(result, context);
        }
    }
}
