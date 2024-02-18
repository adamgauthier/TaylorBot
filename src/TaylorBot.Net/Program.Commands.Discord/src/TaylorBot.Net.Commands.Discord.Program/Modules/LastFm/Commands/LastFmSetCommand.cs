using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Domain;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Strings;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Commands;

public class LastFmSetCommand(ILastFmUsernameRepository lastFmUsernameRepository)
{
    public static readonly CommandMetadata Metadata = new("lastfm set", "Last.fm 🎶", ["fm set", "np set"]);

    public Command Set(IUser user, LastFmUsername lastFmUsername, bool isLegacyCommand) => new(
        Metadata,
        async () =>
        {
            await lastFmUsernameRepository.SetLastFmUsernameAsync(user, lastFmUsername);

            var embed = new EmbedBuilder()
                .WithColor(TaylorBotColors.SuccessColor)
                .WithDescription(
                    $"""
                    Your Last.fm username has been set to {lastFmUsername.Username.DiscordMdLink(lastFmUsername.LinkToProfile)}. ✅
                    You can now use Last.fm commands, get started with </lastfm current:922354806574678086>.
                    """);

            if (isLegacyCommand)
            {
                embed.WithFooter(text: "⭐ Type /lastfm set for an improved command experience!");
            }

            return new EmbedResult(embed.Build());
        }
    );
}

public class LastFmSetSlashCommand(LastFmSetCommand lastFmSetCommand) : ISlashCommand<LastFmSetSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("lastfm set");

    public record Options(LastFmUsername username);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(
            lastFmSetCommand.Set(context.User, options.username, isLegacyCommand: false)
        );
    }
}
