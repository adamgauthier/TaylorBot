using TaylorBot.Net.Commands.Discord.Program.Modules.YouTube.Domain;
using TaylorBot.Net.Commands.PageMessages;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.YouTube.Commands;

public class YouTubeSlashCommand(IYouTubeClient youTubeClient, IRateLimiter rateLimiter, PageMessageFactory pageMessageFactory) : ISlashCommand<YouTubeSlashCommand.Options>
{
    public static string CommandName => "youtube";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public record Options(ParsedString search);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var rateLimitResult = await rateLimiter.VerifyDailyLimitAsync(context.User, "youtube-search");
                if (rateLimitResult != null)
                    return rateLimitResult;

                var result = await youTubeClient.SearchAsync(options.search.Value);

                return result switch
                {
                    SuccessfulSearch search => search.VideoUrls.Count > 0
                        ? pageMessageFactory.Create(new(
                            new(new MessageTextEditor(search.VideoUrls, emptyText: "No YouTube video found for your search 😕")),
                            IsCancellable: true
                        ))
                        : new EmbedResult(EmbedFactory.CreateError("No YouTube video found for your search 😕")),
                    GenericError => new EmbedResult(EmbedFactory.CreateError(
                        """
                        YouTube returned an unexpected error. 😢
                        The site might be down. Try again later!
                        """
                    )),
                    _ => throw new InvalidOperationException(result.GetType().Name),
                };
            }
        ));
    }
}
