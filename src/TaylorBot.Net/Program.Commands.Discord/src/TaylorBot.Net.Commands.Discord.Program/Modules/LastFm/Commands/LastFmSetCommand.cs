using Discord;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Domain;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Strings;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Commands
{
    public class LastFmSetCommand
    {
        public static readonly CommandMetadata Metadata = new("lastfm set", "Last.fm 🎶", new[] { "fm set", "np set" });

        private readonly ILastFmUsernameRepository _lastFmUsernameRepository;

        public LastFmSetCommand(ILastFmUsernameRepository lastFmUsernameRepository)
        {
            _lastFmUsernameRepository = lastFmUsernameRepository;
        }

        public Command Set(IUser user, LastFmUsername lastFmUsername, bool isLegacyCommand) => new(
            Metadata,
            async () =>
            {
                await _lastFmUsernameRepository.SetLastFmUsernameAsync(user, lastFmUsername);

                var embed = new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithDescription(string.Join('\n', new[] {
                        $"Your Last.fm username has been set to {lastFmUsername.Username.DiscordMdLink(lastFmUsername.LinkToProfile)}. ✅",
                        $"You can now use Last.fm commands, get started with **/lastfm current**."
                    }));

                if (isLegacyCommand)
                {
                    embed.WithFooter(text: "⭐ Type /lastfm set for an improved command experience!");
                }

                return new EmbedResult(embed.Build());
            }
        );
    }

    public class LastFmSetSlashCommand : ISlashCommand<LastFmSetSlashCommand.Options>
    {
        public SlashCommandInfo Info => new("lastfm set");

        public record Options(LastFmUsername username);

        private readonly LastFmSetCommand _lastFmSetCommand;

        public LastFmSetSlashCommand(LastFmSetCommand lastFmSetCommand)
        {
            _lastFmSetCommand = lastFmSetCommand;
        }

        public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
        {
            return new(
                _lastFmSetCommand.Set(context.User, options.username, isLegacyCommand: false)
            );
        }
    }
}
