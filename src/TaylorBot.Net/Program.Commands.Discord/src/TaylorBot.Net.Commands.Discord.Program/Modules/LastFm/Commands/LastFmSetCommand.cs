using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Domain;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Strings;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Commands;

public class LastFmSetSlashCommand(ILastFmUsernameRepository lastFmUsernameRepository, CommandMentioner mention) : ISlashCommand<LastFmSetSlashCommand.Options>
{
    public static string CommandName => "lastfm set";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public static readonly CommandMetadata Metadata = new("lastfm set", ["fm set", "np set"]);

    public record Options(LastFmUsername username);

    public Command Set(DiscordUser user, LastFmUsername lastFmUsername, RunContext context) => new(
        context.SlashCommand == null ? Metadata with { IsSlashCommand = false } : Metadata,
        async () =>
        {
            await lastFmUsernameRepository.SetLastFmUsernameAsync(user, lastFmUsername);

            var embed = new EmbedBuilder()
                .WithColor(TaylorBotColors.SuccessColor)
                .WithDescription(
                    $"""
                    Your Last.fm username has been set to {lastFmUsername.Username.DiscordMdLink(lastFmUsername.LinkToProfile)}. ✅
                    You can now use Last.fm commands, get started with {mention.SlashCommand("lastfm current", context)}.
                    """);

            if (context.SlashCommand == null)
            {
                embed.WithFooter(text: "⭐ Type /lastfm set for an improved command experience!");
            }

            return new EmbedResult(embed.Build());
        }
    );

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(Set(context.User, options.username, context));
    }
}
