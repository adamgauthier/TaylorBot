using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Domain;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Commands;

public class LastFmClearSlashCommand(ILastFmUsernameRepository lastFmUsernameRepository, CommandMentioner mention) : ISlashCommand<NoOptions>
{
    public static string CommandName => "lastfm clear";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public static readonly CommandMetadata Metadata = new("lastfm clear", ["fm clear", "np clear"]);

    public Command Clear(DiscordUser user, RunContext context) => new(
        context.SlashCommand == null ? Metadata with { IsSlashCommand = false } : Metadata,
        async () =>
        {
            await lastFmUsernameRepository.ClearLastFmUsernameAsync(user);

            var embed = new EmbedBuilder()
                .WithColor(TaylorBotColors.SuccessColor)
                .WithDescription(
                    $"""
                    Your Last.fm username has been cleared. Last.fm commands will no longer work ✅
                    You can set it again with {mention.SlashCommand("lastfm set", context)}.
                    """);

            if (context.SlashCommand == null)
            {
                embed.WithFooter(text: "⭐ Type /lastfm clear for an improved command experience!");
            }

            return new EmbedResult(embed.Build());
        }
    );

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(Clear(context.User, context));
    }
}
