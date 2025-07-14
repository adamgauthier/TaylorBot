using Discord;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.DiscordInfo.Commands;

public class AvatarSlashCommand(CommandMentioner mention) : ISlashCommand<AvatarSlashCommand.Options>
{
    public static string CommandName => "avatar";

    public static readonly CommandMetadata Metadata = new(CommandName, ["av", "avi"]);

    public ISlashCommandInfo Info => new MessageCommandInfo(Metadata.Name);

    public record Options(ParsedUserOrAuthor user, AvatarType? type);

    public Command Avatar(DiscordUser user, AvatarType? type, RunContext context) => new(
        context.SlashCommand != null ? Metadata : Metadata with { IsSlashCommand = false },
        () =>
        {
            type ??= AvatarType.Guild;
            var avatarUrl = type.Value == AvatarType.Guild
                ? user.GetGuildAvatarUrlOrDefault(size: 2048)
                : user.GetAvatarUrlOrDefault(size: 2048);

            var embed = new EmbedBuilder()
                .WithUserAsAuthor(user)
                .WithColor(TaylorBotColors.SuccessColor)
                .WithImageUrl(avatarUrl);

            if (context.SlashCommand == null)
            {
                embed.WithDescription($"Use {mention.SlashCommand("avatar", context)} instead! 😊");
            }

            return new(new EmbedResult(embed.Build()));
        }
    );

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(Avatar(
            options.user.User,
            options.type,
            context
        ));
    }
}
