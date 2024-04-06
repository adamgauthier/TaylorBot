using Discord;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.DiscordInfo.Commands;

public class AvatarSlashCommand : ISlashCommand<AvatarSlashCommand.Options>
{
    public static readonly CommandMetadata Metadata = new("avatar", "DiscordInfo 💬", ["av", "avi"]);

    public ISlashCommandInfo Info => new MessageCommandInfo(Metadata.Name);

    public record Options(ParsedFetchedUserOrAuthor user, AvatarType? type);

    public Command Avatar(DiscordUser user, AvatarType? type, string? description = null) => new(
        Metadata,
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

            if (description != null)
                embed.WithDescription(description);

            return new(new EmbedResult(embed.Build()));
        }
    );

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(Avatar(
            new(options.user.User),
            options.type
        ));
    }
}
