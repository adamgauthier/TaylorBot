using Discord;
using Humanizer;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Time;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.DiscordInfo.Commands;

public class InspectUserSlashCommand : ISlashCommand<InspectUserSlashCommand.Options>
{
    public static string CommandName => "inspect user";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public record Options(ParsedUserOrAuthor user);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            () =>
            {
                var user = options.user.User;

                var embed = new EmbedBuilder()
                    .WithUserAsAuthor(user, showGuildAvatar: false)
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithThumbnailUrl(user.GetAvatarUrlOrDefault(size: 2048))
                    .AddField("User Id", $"`{user.Id}`", inline: true);

                var createdAt = SnowflakeUtils.FromSnowflake(user.Id);

                if (user.MemberInfo?.JoinedAt.HasValue == true)
                {
                    embed.AddField("Server Joined", user.MemberInfo.JoinedAt.Value.FormatDetailedWithRelative());
                }

                embed.AddField("Account Created", createdAt.FormatDetailedWithRelative());

                if (user.MemberInfo?.Roles.Count > 0)
                {
                    embed.AddField(
                        "Role".ToQuantity(user.MemberInfo.Roles.Count),
                        string.Join(", ", user.MemberInfo.Roles.Take(4).Select(
                            id => MentionUtils.MentionRole(id))) + (user.MemberInfo.Roles.Count > 4 ? ", ..." : string.Empty)
                    );
                }

                return new(new EmbedResult(embed.Build()));
            }
        ));
    }
}
