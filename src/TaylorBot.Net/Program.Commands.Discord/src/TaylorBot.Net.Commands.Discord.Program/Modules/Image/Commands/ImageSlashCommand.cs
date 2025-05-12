using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Image.Domain;
using TaylorBot.Net.Commands.PageMessages;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Image.Commands;

public class ImageSlashCommand(
    IRateLimiter rateLimiter,
    IImageSearchClient imageSearchClient,
    PlusPrecondition.Factory plusPrecondition,
    PageMessageFactory pageMessageFactory) : ISlashCommand<ImageSlashCommand.Options>
{
    public static string CommandName => "image";

    public static readonly CommandMetadata Metadata = new(CommandName, "Media 📷", ["imagen"]);

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public record Options(ParsedString search);

    public Command Image(DiscordUser user, string text) => new(
        Metadata,
        async () =>
        {
            var result = await rateLimiter.VerifyDailyLimitAsync(user, "custom-search");
            if (result != null)
                return result;

            var searchResult = await imageSearchClient.SearchImagesAsync(text);

            switch (searchResult)
            {
                case SuccessfulSearch search:
                    return pageMessageFactory.Create(new(
                        new(new CustomSearchImageEditor(search)),
                        IsCancellable: true));

                case DailyLimitExceeded _:
                    {
                        var embed = new EmbedBuilder()
                            .WithColor(TaylorBotColors.ErrorColor)
                            .WithDescription(
                                """
                                TaylorBot has reached its daily query limit for the underlying image search service. 😢
                                You'll have to wait for the limit to reset. Try again tomorrow!
                                """);

                        return new EmbedResult(embed.Build());
                    }

                case GenericError error:
                    {
                        var embed = new EmbedBuilder()
                            .WithColor(TaylorBotColors.ErrorColor)
                            .WithDescription(
                                """
                                The underlying image search service returned an unexpected error. 😢
                                The site might be down. Try again later!
                                """);

                        return new EmbedResult(embed.Build());
                    }

                default:
                    throw new InvalidOperationException(searchResult.GetType().Name);
            }
        },
        Preconditions: [plusPrecondition.Create(PlusRequirement.PlusUserOrGuild)]
    );

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(Image(context.User, options.search.Value));
    }
}
