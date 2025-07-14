using TaylorBot.Net.Commands.Discord.Program.Modules.YouTube.Domain;
using TaylorBot.Net.Commands.DiscordNet.PageMessages;
using TaylorBot.Net.Commands.PageMessages;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.YouTube.Commands;

public class YouTubeSlashCommand(IYouTubeClient youTubeClient, IRateLimiter rateLimiter, PageMessageFactory pageMessageFactory, CommandMentioner mention) : ISlashCommand<YouTubeSlashCommand.Options>
{
    public static string CommandName => "youtube";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public record Options(ParsedString search);

    public Command Search(DiscordUser author, string query, RunContext context) => new(
        new(Info.Name, IsSlashCommand: context.SlashCommand != null),
        async () =>
        {
            var rateLimitResult = await rateLimiter.VerifyDailyLimitAsync(author, context.SlashCommand == null ? "youtube-search-legacy" : "youtube-search");
            if (rateLimitResult != null)
                return rateLimitResult;

            var result = await youTubeClient.SearchAsync(query);

            switch (result)
            {
                case SuccessfulSearch search:
                    if (context.SlashCommand != null)
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
                                [.. search.VideoUrls.Select(u => $"Use {mention.SlashCommand("youtube")} for a better command experience and higher daily limit.\n{u}")],
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
        new(Search(context.User, options.search.Value, context));
}
