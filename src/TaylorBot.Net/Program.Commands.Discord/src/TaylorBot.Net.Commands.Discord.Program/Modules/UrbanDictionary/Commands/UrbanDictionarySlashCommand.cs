using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.UrbanDictionary.Domain;
using TaylorBot.Net.Commands.DiscordNet.PageMessages;
using TaylorBot.Net.Commands.PageMessages;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.UrbanDictionary.Commands;

public class UrbanDictionaryCommand(IUrbanDictionaryClient urbanDictionaryClient, IRateLimiter rateLimiter)
{
    public static readonly CommandMetadata Metadata = new("urbandictionary", "Knowledge ❓", new[] { "urban" });

    public Command Search(IUser author, string query, bool isLegacyCommand = false) => new(
        Metadata,
        async () =>
        {
            var rateLimitResult = await rateLimiter.VerifyDailyLimitAsync(author, isLegacyCommand ? "urbandictionary-search-legacy" : "urbandictionary-search");
            if (rateLimitResult != null)
                return rateLimitResult;

            var result = await urbanDictionaryClient.SearchAsync(query);

            switch (result)
            {
                case UrbanDictionaryResult searchResult:
                    if (!isLegacyCommand)
                    {
                        return new PageMessageResultBuilder(new(
                            new(new UrbanDictionaryEditor(searchResult)),
                            IsCancellable: true
                        )).Build();
                    }
                    else
                    {
                        EmbedBuilder BuildBaseEmbed() =>
                            new EmbedBuilder().WithUserAsAuthor(author);

                        return new PageMessageResult(new PageMessage(new(
                            new EmbedPageMessageRenderer(new UrbanDictionaryEditor(searchResult), BuildBaseEmbed),
                            Cancellable: true
                        )));
                    }

                case GenericUrbanError error:
                    return new EmbedResult(EmbedFactory.CreateError(
                        """                        
                        UrbanDictionary returned an unexpected error. 😢
                        The site might be down. Try again later!
                        """
                    ));

                default:
                    throw new InvalidOperationException(result.GetType().Name);
            };
        }
    );
}

public class UrbanDictionarySlashCommand(UrbanDictionaryCommand urbanDictionaryCommand) : ISlashCommand<UrbanDictionarySlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("urbandictionary");

    public record Options(ParsedString search);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(
            urbanDictionaryCommand.Search(
                context.User,
                options.search.Value
            )
        );
    }
}
