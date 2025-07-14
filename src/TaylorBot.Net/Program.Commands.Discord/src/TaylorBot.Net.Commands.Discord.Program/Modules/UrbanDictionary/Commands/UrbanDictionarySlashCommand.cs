using TaylorBot.Net.Commands.Discord.Program.Modules.UrbanDictionary.Domain;
using TaylorBot.Net.Commands.PageMessages;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.UrbanDictionary.Commands;

public class UrbanDictionarySlashCommand(
    IUrbanDictionaryClient urbanDictionaryClient,
    IRateLimiter rateLimiter,
    PageMessageFactory pageMessageFactory) : ISlashCommand<UrbanDictionarySlashCommand.Options>
{
    public static readonly CommandMetadata Metadata = new("urbandictionary", ["urban"]);

    public static string CommandName => "urbandictionary";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public record Options(ParsedString search);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(Search(context.User, options.search.Value));
    }

    private Command Search(DiscordUser author, string query) => new(
        Metadata,
        async () =>
        {
            var rateLimitResult = await rateLimiter.VerifyDailyLimitAsync(author, "urbandictionary-search");
            if (rateLimitResult != null)
                return rateLimitResult;

            var result = await urbanDictionaryClient.SearchAsync(query);

            switch (result)
            {
                case UrbanDictionaryResult searchResult:
                    return pageMessageFactory.Create(new(
                        new(new UrbanDictionaryEditor(searchResult)),
                        IsCancellable: true
                    ));

                case GenericUrbanError error:
                    return new EmbedResult(EmbedFactory.CreateError(
                        """
                        UrbanDictionary returned an unexpected error. 😢
                        The site might be down. Try again later!
                        """
                    ));

                default:
                    throw new InvalidOperationException(result.GetType().Name);
            }
        }
    );
}
