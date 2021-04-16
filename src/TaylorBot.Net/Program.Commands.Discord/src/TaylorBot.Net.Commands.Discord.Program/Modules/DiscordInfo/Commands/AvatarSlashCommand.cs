using Discord;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Events;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.DiscordInfo.Commands
{
    public class AvatarCommand
    {
        public static readonly CommandMetadata Metadata = new("avatar", "DiscordInfo 💬", new[] { "av", "avi" });

        public Command Avatar(IUser user, string? footer = null) => new(
            Metadata,
            () =>
            {
                var embed = new EmbedBuilder()
                    .WithUserAsAuthor(user)
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithImageUrl(user.GetAvatarUrlOrDefault(size: 2048));

                if (footer != null)
                    embed.WithFooter(footer);

                return new(new EmbedResult(embed.Build()));
            }
        );
    }

    public class AvatarSlashCommand : ISlashCommand
    {
        private readonly ITaylorBotClient _taylorBotClient;

        public AvatarSlashCommand(ITaylorBotClient taylorBotClient)
        {
            _taylorBotClient = taylorBotClient;
        }

        public string Name => AvatarCommand.Metadata.Name;

        public async ValueTask<Command> GetCommandAsync(RunContext context, Interaction.ApplicationCommandInteractionData data)
        {
            var userParameter = (JsonElement?)data.options?.Single(option => option.name == "user")?.value;

            var targetUser = userParameter.HasValue ?
                await _taylorBotClient.ResolveRequiredUserAsync(new(userParameter.Value.GetString()!)) :
                context.User;

            return new AvatarCommand().Avatar(targetUser);
        }
    }
}
