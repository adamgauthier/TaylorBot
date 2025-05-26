using TaylorBot.Net.Commands.Discord.Program.Modules.YouTube.Domain;
using TaylorBot.Net.Commands.DiscordNet.PageMessages;
using TaylorBot.Net.Commands.PageMessages;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.YouTube.Commands;

public class YouTubeSlashCommand(IYouTubeClient youTubeClient, IRateLimiter rateLimiter, PageMessageFactory pageMessageFactory) : ISlashCommand<YouTubeSlashCommand.Options>
{
    public static string CommandName => "youtube";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public record Options(ParsedString search);

    public Command Search(DiscordUser author, string query, bool isLegacyCommand = false) => new(
        new(Info.Name),
        async () =>
        {
            var rateLimitResult = await rateLimiter.VerifyDailyLimitAsync(author, isLegacyCommand ? "youtube-search-legacy" : "youtube-search");
            if (rateLimitResult != null)
                return rateLimitResult;

            var result = await youTubeClient.SearchAsync(query);

            switch (result)
            {
                case SuccessfulSearch search:
                    if (!isLegacyCommand)
                    {
                        return search.VideoUrls.Count > 0
                            ? pageMessageFactory.Create(new(
                                new(new MessageTextEditor(search.VideoUrls, emptyText: "No YouTube video found for your search 😕")),
                                IsCancellable: true
                            ))
                            : new EmbedResult(EmbedFactory.CreateError("No YouTube video found for your search 😕"));
                    }
                    else
                    {
                        return new PageMessageResult(new PageMessage(new(
                            new TextPageMessageRenderer(new(
                                [.. search.VideoUrls.Select(u => $"Use </youtube:861754955728027679> for a better command experience and higher daily limit.\n{u}")],
                                emptyText: "No YouTube video found for your search 😕")),
                            Cancellable: true
                        )));
                    }

                case GenericError error:
                    return new EmbedResult(EmbedFactory.CreateError(
                        """
                        YouTube returned an unexpected error. 😢
                        The site might be down. Try again later!
                        """
                    ));

                default:
                    throw new InvalidOperationException(result.GetType().Name);
            }
            ;
        }
    );

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options) =>
        new(Search(context.User, options.search.Value));
}
