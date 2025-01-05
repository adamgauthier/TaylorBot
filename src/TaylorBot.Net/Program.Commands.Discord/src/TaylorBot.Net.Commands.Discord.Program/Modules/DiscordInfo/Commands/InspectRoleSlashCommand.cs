using Discord;
using Humanizer;
using TaylorBot.Net.Commands.Parsers.Roles;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Time;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.DiscordInfo.Commands;

public class InspectRoleSlashCommand : ISlashCommand<InspectRoleSlashCommand.Options>
{
    public static string CommandName => "inspect role";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public record Options(ParsedRole role);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var role = options.role.Role;

                var embed = new EmbedBuilder()
                    .WithColor(role.color)
                    .WithAuthor(role.name)
                    .AddField("Id", $"`{role.id}`", inline: true)
                    .AddField("Color", $"`{role.color}` (`{new Color(role.color)}`)", inline: true)
                    .AddField("Hoist", role.hoist ? "✅" : "❌", inline: true)
                    .AddField("Icon", role.icon ?? role.unicode_emoji ?? "None", inline: true)
                    .AddField("Position", $"`{role.position}`", inline: true)
                    .AddField("Permissions", $"`{role.permissions}`", inline: true)
                    .AddField("Managed", role.managed ? "✅" : "❌", inline: true)
                    .AddField("Mentionable", role.mentionable ? "✅" : "❌", inline: true)
                    .AddField("Flags", $"`{role.flags}`", inline: true)
                    .AddField("Created", SnowflakeUtils.FromSnowflake(role.id).FormatDetailedWithRelative());

                var guild = context.Guild?.Fetched;
                if (guild != null)
                {
                    var members = (await guild.GetUsersAsync(CacheMode.CacheOnly))
                        .Where(m => m.RoleIds.Contains(role.id))
                        .ToList();

                    embed.AddField("Members",
                        $"**({members.Count}+)**{(members.Count != 0 ? $" {string.Join(", ", members.Select(m => m.Nickname ?? m.Username)).Truncate(100)}" : "")}");
                }

                return new EmbedResult(embed.Build());
            }
        ));
    }
}
