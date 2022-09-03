using Discord;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.DiscordInfo.Commands
{
    public class AvatarCommand
    {
        public static readonly CommandMetadata Metadata = new("avatar", "DiscordInfo 💬", new[] { "av", "avi" });

        public Command Avatar(IUser user, string? description = null) => new(
            Metadata,
            () =>
            {
                var embed = new EmbedBuilder()
                    .WithUserAsAuthor(user)
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithImageUrl(user.GetAvatarUrlOrDefault(size: 2048));

                if (description != null)
                    embed.WithDescription(description);

                return new(new EmbedResult(embed.Build()));
            }
        );
    }

    public class AvatarSlashCommand : ISlashCommand<AvatarSlashCommand.Options>
    {
        public SlashCommandInfo Info => new(AvatarCommand.Metadata.Name);

        public record Options(ParsedUserOrAuthor user);

        public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
        {
            return new(new AvatarCommand().Avatar(options.user.User));
        }
    }
}
