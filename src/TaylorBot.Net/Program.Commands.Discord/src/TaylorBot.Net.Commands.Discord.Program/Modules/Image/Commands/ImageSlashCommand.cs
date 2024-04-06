using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Image.Domain;
using TaylorBot.Net.Commands.DiscordNet.PageMessages;
using TaylorBot.Net.Commands.PageMessages;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Image.Commands;

public class ImageCommand(IPlusRepository plusRepository, IRateLimiter rateLimiter, IImageSearchClient imageSearchClient)
{
    public static readonly CommandMetadata Metadata = new("image", "Media 📷", ["imagen"]);

    public Command Image(DiscordUser user, string text, bool isLegacyCommand) => new(
        Metadata,
        async () =>
        {
            var action = isLegacyCommand ? "custom-search-legacy" : "custom-search";

            var result = await rateLimiter.VerifyDailyLimitAsync(user, action);
            if (result != null)
                return result;

            var searchResult = await imageSearchClient.SearchImagesAsync(text);

            switch (searchResult)
            {
                case SuccessfulSearch search:
                    if (isLegacyCommand)
                    {
                        EmbedBuilder BuildBaseEmbed() =>
                            new EmbedBuilder()
                                .WithColor(TaylorBotColors.SuccessColor)
                                .WithDescription("Use </image:870731803739168860> instead! 😊");

                        return new PageMessageResult(new PageMessage(new(
                            new EmbedPageMessageRenderer(new CustomSearchImagePageEditor(search.Images), BuildBaseEmbed),
                            Cancellable: true
                        )));
                    }
                    else
                    {
                        return new PageMessageResultBuilder(new(
                            new(new CustomSearchImageEditor(search)),
                            IsCancellable: true
                        )).Build();
                    }

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
        Preconditions: [new PlusPrecondition(plusRepository, PlusRequirement.PlusUserOrGuild)]
    );
}

public class ImageSlashCommand(IPlusRepository plusRepository, IRateLimiter rateLimiter, IImageSearchClient imageSearchClient) : ISlashCommand<ImageSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo(ImageCommand.Metadata.Name);

    public record Options(ParsedString search);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        var command = new ImageCommand(plusRepository, rateLimiter, imageSearchClient);
        return new(command.Image(context.User, options.search.Value, isLegacyCommand: false));
    }
}
