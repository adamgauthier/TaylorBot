using Discord;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.UrbanDictionary.Domain;
using TaylorBot.Net.Commands.DiscordNet.PageMessages;
using TaylorBot.Net.Commands.PageMessages;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.UrbanDictionary.Commands;

public class UrbanDictionaryCommand
{
    public static readonly CommandMetadata Metadata = new("urbandictionary", "Knowledge ❓", new[] { "urban" });

    private readonly IUrbanDictionaryClient _urbanDictionaryClient;
    private readonly IRateLimiter _rateLimiter;

    public UrbanDictionaryCommand(IUrbanDictionaryClient urbanDictionaryClient, IRateLimiter rateLimiter)
    {
        _urbanDictionaryClient = urbanDictionaryClient;
        _rateLimiter = rateLimiter;
    }

    public Command Search(IUser author, string query, bool isLegacyCommand = false) => new(
        Metadata,
        async () =>
        {
            var rateLimitResult = await _rateLimiter.VerifyDailyLimitAsync(author, isLegacyCommand ? "urbandictionary-search-legacy" : "urbandictionary-search");
            if (rateLimitResult != null)
                return rateLimitResult;

            var result = await _urbanDictionaryClient.SearchAsync(query);

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
                            new EmbedBuilder()
                                .WithUserAsAuthor(author);

                        return new PageMessageResult(new PageMessage(new(
                            new EmbedPageMessageRenderer(new UrbanDictionaryEditor(searchResult), BuildBaseEmbed),
                            Cancellable: true
                        )));
                    }

                case GenericUrbanError error:
                    return new EmbedResult(EmbedFactory.CreateError(string.Join('\n', new[] {
                        "UrbanDictionary returned an unexpected error. 😢",
                        "The site might be down. Try again later!"
                    })));

                default:
                    throw new InvalidOperationException(result.GetType().Name);
            };
        }
    );
}

public class UrbanDictionarySlashCommand : ISlashCommand<UrbanDictionarySlashCommand.Options>
{
    public SlashCommandInfo Info => new("urbandictionary");

    public record Options(ParsedString search);

    private readonly UrbanDictionaryCommand _urbanDictionaryCommand;

    public UrbanDictionarySlashCommand(UrbanDictionaryCommand urbanDictionaryCommand)
    {
        _urbanDictionaryCommand = urbanDictionaryCommand;
    }

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(
            _urbanDictionaryCommand.Search(
                context.User,
                options.search.Value
            )
        );
    }
}
