using Discord;
using Microsoft.Extensions.Logging;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Commands;

public interface IBirthdayRoleConfigRepository
{
    Task<string?> GetRoleForGuildAsync(IGuild guild);
    Task AddRoleForGuildAsync(IGuild guild, IRole role);
    Task RemoveRoleForGuildAsync(IGuild guild);
}

public class BirthdayRoleSlashCommand(
    IBirthdayRoleConfigRepository birthdayRoleRepository,
    UserHasPermissionOrOwnerPrecondition.Factory userHasPermission,
    TaylorBotHasPermissionPrecondition.Factory botHasPermission,
    PlusPrecondition.Factory plusPrecondition,
    CommandMentioner mention) : ISlashCommand<NoOptions>
{
    public static string CommandName => "birthday role";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public IList<ICommandPrecondition> BuildPreconditions() => [
        plusPrecondition.Create(PlusRequirement.PlusGuild),
        botHasPermission.Create(GuildPermission.ManageRoles),
        userHasPermission.Create(GuildPermission.ManageRoles),
    ];

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions _)
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
                    if (role != null)
                    {
                        return new MessageResult(new(
                            new(new EmbedBuilder()
                                .WithColor(TaylorBotColors.SuccessColor)
                                .WithGuildAsAuthor(guild)
                                .WithDescription(
                                    $"""
                                    The birthday role for this server is {role.Mention} ✅
                                    {HowItWorks(context)}
                                    """)
                                .Build()),
                            [new Button(InteractionCustomId.Create(BirthdayRoleRemoveButtonHandler.CustomIdName).RawId, ButtonStyle.Danger, Label: "Remove birthday role", Emoji: "🗑️")]));
                    }
                    else
                    {
                        return new MessageResult(new(
                            new(new EmbedBuilder()
                                .WithColor(TaylorBotColors.SuccessColor)
                                .WithGuildAsAuthor(guild)
                                .WithDescription(
                                    """
                                    There used to be a birthday role for this server, but it is now deleted 😮
                                    What do you want to do? 🤔
                                    """)
                                .Build()),
                            [
                                new Button(InteractionCustomId.Create(BirthdayRoleCreateButtonHandler.CustomIdName).RawId, ButtonStyle.Primary, Label: "Re-create birthday role", Emoji: "🎂"),
                                new Button(InteractionCustomId.Create(BirthdayRoleRemoveButtonHandler.CustomIdName).RawId, ButtonStyle.Danger, Label: "Remove birthday role", Emoji: "🗑️"),
                            ]));
                    }
                }
                else
                {
                    return new MessageResult(new(
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
                        [
                            new Button(InteractionCustomId.Create(BirthdayRoleCreateButtonHandler.CustomIdName).RawId, ButtonStyle.Primary, Label: "Create birthday role", Emoji: "🎂")
                        ]));
                }
            },
            Preconditions: BuildPreconditions()
        ));
    }

    public string HowItWorks(RunContext context) =>
        $"""
        ### How Does It Work ❓
        - A member sets their birthday with {mention.SlashCommand("birthday set", context)} 🎂
        - On their birthday, **they automatically get the role** 🎈
        - When the birthday is over, **the role is removed automatically** 🥲

        The role is kept for **~40 hours** so the birthday is celebrated in all timezones 🌐
        Members can **NOT** change their birthday to get the role multiple times a year 🚫
        """;
}

public class BirthdayRoleCreateButtonHandler(
    IInteractionResponseClient responseClient,
    IBirthdayRoleConfigRepository birthdayRoleRepository,
    BirthdayRoleSlashCommand birthdayRoleSlashCommand) : IButtonHandler
{
    public static CustomIdNames CustomIdName => CustomIdNames.BirthdayRoleCreate;

    public IComponentHandlerInfo Info => new MessageHandlerInfo(
        CustomIdName.ToText(),
        Preconditions: birthdayRoleSlashCommand.BuildPreconditions(),
        RequireOriginalUser: true);

    public async Task HandleAsync(DiscordButtonComponent button, RunContext context)
    {
        var guild = context.Guild?.Fetched;
        ArgumentNullException.ThrowIfNull(guild);

        Emoji? emoji = guild.Features.HasRoleIcons ? new("🎂") : null;
        var role = await guild.CreateRoleAsync("happy birthday", color: DiscordColor.FromHexString("#3498DB"), isHoisted: true, emoji: emoji);

        await birthdayRoleRepository.AddRoleForGuildAsync(guild, role);

        var embed = new EmbedBuilder()
            .WithColor(TaylorBotColors.SuccessColor)
            .WithGuildAsAuthor(guild)
            .WithDescription(
                $"""
                Birthday role created: {role.Mention} ✅
                Feel free to **change the name, color, order, etc.** 🖌️
                {birthdayRoleSlashCommand.HowItWorks(context)}
                """)
            .Build();

        await responseClient.EditOriginalResponseAsync(button.Interaction, InteractionMapper.ToInteractionEmbed(embed));
    }
}

public partial class BirthdayRoleRemoveButtonHandler(
    ILogger<BirthdayRoleRemoveButtonHandler> logger,
    IInteractionResponseClient responseClient,
    IBirthdayRoleConfigRepository birthdayRoleRepository,
    BirthdayRoleSlashCommand birthdayRoleSlashCommand,
    CommandMentioner mention
) : IButtonHandler
{
    public static CustomIdNames CustomIdName => CustomIdNames.BirthdayRoleRemove;

    public IComponentHandlerInfo Info => new MessageHandlerInfo(
        CustomIdName.ToText(),
        Preconditions: birthdayRoleSlashCommand.BuildPreconditions(),
        RequireOriginalUser: true);

    public async Task HandleAsync(DiscordButtonComponent button, RunContext context)
    {
        var guild = context.Guild?.Fetched;
        ArgumentNullException.ThrowIfNull(guild);

        var roleId = await birthdayRoleRepository.GetRoleForGuildAsync(guild);
        var role = roleId != null ? guild.GetRole(new SnowflakeId(roleId)) : null;

        await birthdayRoleRepository.RemoveRoleForGuildAsync(guild);

        if (role != null)
        {
            try
            {
                await role.DeleteAsync();
            }
            catch (Exception e)
            {
                LogUnhandledExceptionDeletingBirthdayRole(e, role.Id, guild.FormatLog());
            }
        }

        await responseClient.EditOriginalResponseAsync(button.Interaction, EmbedFactory.CreateSuccessEmbed(
            $"""
            Successfully **removed birthday role from this server** ✅
            Members will no longer automatically receive a role on their birthday 🎂
            Use {mention.SlashCommand("birthday role", context)} again to re-create the role 🎈
            """));
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception when deleting birthday role {RoleId} in {Guild}")]
    private partial void LogUnhandledExceptionDeletingBirthdayRole(Exception exception, ulong roleId, string guild);
}
