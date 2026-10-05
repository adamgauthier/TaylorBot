using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Domain;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Commands;

public class LastFmClearSlashCommand(ILastFmUsernameRepository lastFmUsernameRepository, CommandMentioner mention) : ISlashCommand<NoOptions>
{
    public static string CommandName => "lastfm clear";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                await lastFmUsernameRepository.ClearLastFmUsernameAsync(context.User);

                var embed = new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithDescription(
                        await mention.FormatAsync(context, $"""
                        Your Last.fm username has been cleared. Last.fm commands will no longer work ✅
                        You can set it again with {mention.Slash("lastfm set")}.
                        """));

                return new EmbedResult(embed.Build());
            }
        ));
    }
}
