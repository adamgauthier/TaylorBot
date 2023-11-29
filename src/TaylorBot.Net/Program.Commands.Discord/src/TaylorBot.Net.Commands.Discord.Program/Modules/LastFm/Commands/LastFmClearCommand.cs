using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Domain;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Commands
{
    public class LastFmClearCommand
    {
        public static readonly CommandMetadata Metadata = new("lastfm clear", "Last.fm 🎶", new[] { "fm clear", "np clear" });

        private readonly ILastFmUsernameRepository _lastFmUsernameRepository;

        public LastFmClearCommand(ILastFmUsernameRepository lastFmUsernameRepository)
        {
            _lastFmUsernameRepository = lastFmUsernameRepository;
        }

        public Command Clear(IUser user, bool isLegacyCommand) => new(
            Metadata,
            async () =>
            {
                await _lastFmUsernameRepository.ClearLastFmUsernameAsync(user);

                var embed = new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithDescription(string.Join('\n', new[] {
                        $"Your Last.fm username has been cleared. Last.fm commands will no longer work. ✅",
                        $"You can set it again with </lastfm set:922354806574678086>."
                    }));

                if (isLegacyCommand)
                {
                    embed.WithFooter(text: "⭐ Type /lastfm clear for an improved command experience!");
                }

                return new EmbedResult(embed.Build());
            }
        );
    }

    public class LastFmClearSlashCommand : ISlashCommand<NoOptions>
    {
        public ISlashCommandInfo Info => new MessageCommandInfo("lastfm clear");

        private readonly LastFmClearCommand _lastFmClearCommand;

        public LastFmClearSlashCommand(LastFmClearCommand lastFmClearCommand)
        {
            _lastFmClearCommand = lastFmClearCommand;
        }

        public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
        {
            return new(
                _lastFmClearCommand.Clear(context.User, isLegacyCommand: false)
            );
        }
    }
}
