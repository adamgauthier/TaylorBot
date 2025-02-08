using Discord;
using Microsoft.Extensions.Logging;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.Core.Snowflake;
using static TaylorBot.Net.Commands.MessageResult;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Commands;

public interface IBirthdayRoleConfigRepository
{
    Task<string?> GetRoleForGuildAsync(IGuild guild);
    Task AddRoleForGuildAsync(IGuild guild, IRole role);
    Task RemoveRoleForGuildAsync(IGuild guild);
}

public class BirthdayRoleSlashCommand(
    ILogger<BirthdayRoleSlashCommand> logger,
    IPlusRepository plusRepository,
    IBirthdayRoleConfigRepository birthdayRoleRepository,
    UserHasPermissionOrOwnerPrecondition.Factory userHasPermission) : ISlashCommand<NoOptions>
{
    public static string CommandName => "birthday role";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var guild = context.Guild?.Fetched;
                ArgumentNullException.ThrowIfNull(guild);

                var roleId = await birthdayRoleRepository.GetRoleForGuildAsync(guild);
                if (roleId is not null)
                {
                    var role = guild.GetRole(new SnowflakeId(roleId));
                    if (role is not null)
                    {
                        return new MessageResult(
                            new(new EmbedBuilder()
                                .WithColor(TaylorBotColors.SuccessColor)
                                .WithGuildAsAuthor(guild)
                                .WithDescription(
                                    $"""
                                    The birthday role for this server is {role.Mention} ✅
                                    {HowItWorks(context)}
                                    """)
                                .Build()),
                            new([
                                new ButtonResult(new("stop", ButtonStyle.Danger, Label: "Remove birthday role", Emoji: "🗑️"), _ => RemoveAsync(context, guild, role)),
                            ]));
                    }
                    else
                    {
                        return new MessageResult(
                            new(new EmbedBuilder()
                                .WithColor(TaylorBotColors.SuccessColor)
                                .WithGuildAsAuthor(guild)
                                .WithDescription(
                                    """
                                    There used to be a birthday role for this server, but it is now deleted 😮
                                    What do you want to do? 🤔
                                    """)
                                .Build()),
                            new([
                                new ButtonResult(new("recreate", ButtonStyle.Primary, Label: "Re-create birthday role", Emoji: "🎂"), _ => CreateAsync(context, guild)),
                                new ButtonResult(new("stop", ButtonStyle.Danger, Label: "Remove birthday role", Emoji: "🗑️"), _ => RemoveAsync(context, guild, role)),
                            ]));
                    }
                }
                else
                {
                    return new MessageResult(
                        new(new EmbedBuilder()
                            .WithColor(TaylorBotColors.SuccessColor)
                            .WithGuildAsAuthor(guild)
                            .WithDescription(
                                $"""
                                There is no birthday role in this server 😕
                                Use the **button below** to **create one** ✅
                                {HowItWorks(context)}
                                """)
                            .Build()),
                        new([
                            new ButtonResult(new("create", ButtonStyle.Primary, Label: "Create birthday role", Emoji: "🎂"), _ => CreateAsync(context, guild)),
                        ]));
                }
            },
            Preconditions: [
                new PlusPrecondition(plusRepository, PlusRequirement.PlusGuild),
                new TaylorBotHasPermissionPrecondition(GuildPermission.ManageRoles),
                userHasPermission.Create(GuildPermission.ManageRoles),
            ]
        ));
    }

    private async ValueTask<IButtonClickResult> CreateAsync(RunContext context, IGuild guild)
    {
        Emoji? emoji = guild.Features.HasRoleIcons ? new("🎂") : null;
        var role = await guild.CreateRoleAsync("happy birthday", color: DiscordColor.FromHexString("#3498DB"), isHoisted: true, emoji: emoji);

        await birthdayRoleRepository.AddRoleForGuildAsync(guild, role);

        return new UpdateMessage(new(new MessageContent(new EmbedBuilder()
            .WithColor(TaylorBotColors.SuccessColor)
            .WithGuildAsAuthor(guild)
            .WithDescription(
                $"""
                Birthday role created: {role.Mention} ✅
                Feel free to **change the name, color, order, etc.** 🖌️
                {HowItWorks(context)}
                """).Build())));
    }

    private async ValueTask<IButtonClickResult> RemoveAsync(RunContext context, IGuild guild, IRole? role)
    {
        await birthdayRoleRepository.RemoveRoleForGuildAsync(guild);

        if (role != null)
        {
            try
            {
                await role.DeleteAsync();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unhandled exception when deleting birthday role {RoleId} in {Guild}", role.Id, guild.FormatLog());
            }
        }

        return new UpdateMessage(new(new MessageContent(EmbedFactory.CreateSuccess(
            $"""
            Successfully **removed birthday role from this server** ✅
            Members will no longer automatically receive a role on their birthday 🎂
            Use {context.MentionCommand("birthday role")} again to re-create the role 🎈
            """))));
    }

    private static string HowItWorks(RunContext context) =>
        $"""
        ### How Does It Work ❓
        - A member sets their birthday with {context.MentionCommand("birthday set")} 🎂
        - On their birthday, **they automatically get the role** 🎈
        - When the birthday is over, **the role is removed automatically** 🥲

        The role is kept for **~40 hours** so the birthday is celebrated in all timezones 🌐
        Members can **NOT** change their birthday to get the role multiple times a year 🚫
        """;
}
