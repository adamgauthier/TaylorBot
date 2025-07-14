using Discord;
using Discord.Commands;
using Discord.Net;
using Humanizer;
using System.Net;
using TaylorBot.Net.Commands.Discord.Program.Modules.AccessibleRoles.Domain;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Colors;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.AccessibleRoles.Commands;

[Name("Roles 🆔")]
[Group("roles")]
[Alias("role", "gr")]
public class AccessibleRolesModule(
    ICommandRunner commandRunner,
    IAccessibleRoleRepository accessibleRoleRepository,
    UserHasPermissionOrOwnerPrecondition.Factory userHasPermission,
    TaylorBotHasPermissionPrecondition.Factory botHasPermission) : TaylorBotModule
{
    [Priority(-1)]
    [Command]
    public async Task<RuntimeResult> GetAsync(
        [Remainder]
        RoleNotEveryoneArgument<IRole>? role = null
    )
    {
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            async () =>
            {
                var member = (IGuildUser)Context.User;
                EmbedBuilder embed = new();

                if (role != null)
                {
                    if (!member.RoleIds.Contains(role.Role.Id))
                    {
                        var accessibleRole = await accessibleRoleRepository.GetAccessibleRoleAsync(role.Role);
                        if (accessibleRole != null)
                        {
                            var groupInfo = accessibleRole.Group != null ?
                                new
                                {
                                    accessibleRole.Group,
                                    MemberRolesInSameGroup = member.RoleIds.Intersect(accessibleRole.Group.OtherRoles.Select(r => r.Id)).ToList()
                                } :
                                null;

                            if (groupInfo != null && groupInfo.MemberRolesInSameGroup.Count != 0)
                            {
                                embed
                                    .WithColor(TaylorBotColors.ErrorColor)
                                    .WithDescription(
                                        $"""
                                        Sorry, {role.Role.Mention} is part of the '{groupInfo.Group.Name}' group.
                                        You already have {MentionUtils.MentionRole(groupInfo.MemberRolesInSameGroup.First())} which is part of the same group.
                                        Use `{Context.CommandPrefix}role drop` to drop it!
                                        """);
                            }
                            else
                            {
                                try
                                {
                                    await member.AddRoleAsync(role.Role, new RequestOptions
                                    {
                                        AuditLogReason = $"Assigned accessible role on user's request with message id {Context.Message.Id}."
                                    });

                                    embed
                                        .WithColor(TaylorBotColors.SuccessColor)
                                        .WithDescription(
                                            $"""
                                            You now have {role.Role.Mention}. 😊
                                            Use `{Context.CommandPrefix}role drop {role.Role.Name}` to drop it!
                                            """);
                                }
                                catch (HttpException exception) when (exception.HttpCode == HttpStatusCode.Forbidden)
                                {
                                    embed
                                        .WithColor(TaylorBotColors.ErrorColor)
                                        .WithDescription(
                                            $"""
                                            Sorry, Discord does not allow me to give you {role.Role.Mention}.
                                            Make sure my bot role is before {role.Role.Mention} in the roles order under server settings!
                                            """);
                                }
                            }
                        }
                        else
                        {
                            embed
                                .WithColor(TaylorBotColors.ErrorColor)
                                .WithDescription(
                                    $"""
                                    Sorry, {role.Role.Mention} is not marked as accessible so I can't give it to you.
                                    Use `{Context.CommandPrefix}roles add {role.Role.Name}` to make it accessible to everyone!
                                    """);
                        }
                    }
                    else
                    {
                        embed
                            .WithColor(TaylorBotColors.ErrorColor)
                            .WithDescription(
                                $"""
                                You already have role {role.Role.Mention}.
                                Use `{Context.CommandPrefix}role drop {role.Role.Name}` to drop it!
                                """);
                    }
                }
                else
                {
                    var accessibleRoles = (await accessibleRoleRepository.GetAccessibleRolesAsync(Context.Guild))
                        .Where(ar => Context.Guild.Roles.Any(r => r.Id == ar.RoleId.Id))
                        .ToList();

                    embed.WithColor(TaylorBotColors.SuccessColor);

                    if (accessibleRoles.Count != 0)
                    {
                        var ungrouped = accessibleRoles.Where(ar => ar.GroupName == null).ToList();
                        var grouped = accessibleRoles.Where(ar => ar.GroupName != null).GroupBy(ar => ar.GroupName);

                        embed.WithDescription($"Here are the accessible roles in this server, use `{Context.CommandPrefix}role role-name` to get one of them.");

                        if (ungrouped.Count != 0)
                        {
                            embed.AddField("no group", string.Join(", ", ungrouped.Select(r => MentionUtils.MentionRole(r.RoleId.Id))).Truncate(EmbedFieldBuilder.MaxFieldValueLength), inline: false);
                        }

                        foreach (var group in grouped.Take(EmbedBuilder.MaxFieldCount - embed.Fields.Count))
                        {
                            embed.AddField(group.Key, string.Join(", ", group.Select(r => MentionUtils.MentionRole(r.RoleId.Id))).Truncate(EmbedFieldBuilder.MaxFieldValueLength), inline: true);
                        }
                    }
                    else
                    {
                        embed.WithDescription(
                            $"""
                            There is currently no accessible role in this server.
                            Accessible roles are roles that everyone has access to using `{Context.CommandPrefix}role`.
                            Use `{Context.CommandPrefix}roles add role-name` to add one!
                            """);
                    }

                }

                return new EmbedResult(embed.Build());
            },
            Preconditions: [botHasPermission.Create(GuildPermission.ManageRoles)]
        );

        var context = DiscordNetContextMapper.MapToRunContext(Context, new());
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("drop")]
    public async Task<RuntimeResult> DropAsync(
        [Remainder]
        RoleNotEveryoneArgument<IRole> role
    )
    {
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            async () =>
            {
                var member = (IGuildUser)Context.User;
                EmbedBuilder embed = new();

                if (member.RoleIds.Contains(role.Role.Id))
                {
                    if (await accessibleRoleRepository.IsRoleAccessibleAsync(role.Role))
                    {
                        await member.RemoveRoleAsync(role.Role, new RequestOptions
                        {
                            AuditLogReason = $"Removed accessible role on user's request with message id {Context.Message.Id}."
                        });

                        embed
                            .WithColor(TaylorBotColors.SuccessColor)
                            .WithDescription(
                                $"""
                                Removed {role.Role.Mention} from your roles. 😊
                                Use `{Context.CommandPrefix}role {role.Role.Name}` to get it back!
                                """);
                    }
                    else
                    {
                        embed
                            .WithColor(TaylorBotColors.ErrorColor)
                            .WithDescription(
                                $"""
                                Sorry, {role.Role.Mention} is not accessible so you can't drop it.
                                Use `{Context.CommandPrefix}roles add {role.Role.Name}` to make it accessible to everyone!
                                """);
                    }
                }
                else
                {
                    embed
                        .WithColor(TaylorBotColors.ErrorColor)
                        .WithDescription($"You don't have the role {role.Role.Mention} so you can't drop it!");
                }

                return new EmbedResult(embed.Build());
            },
            Preconditions: [botHasPermission.Create(GuildPermission.ManageRoles)]
        );

        var context = DiscordNetContextMapper.MapToRunContext(Context, new());
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("add")]
    public async Task<RuntimeResult> AddAsync(
        [Remainder]
        RoleNotEveryoneArgument<IRole> role
    )
    {
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            async () =>
            {
                await accessibleRoleRepository.AddAccessibleRoleAsync(role.Role);

                return new EmbedResult(new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithDescription(
                        $"""
                        Successfully made {role.Role.Mention} accessible to everyone in the server. 😊
                        Use `{Context.CommandPrefix}role {role.Role.Name}` to get it!
                        Use `{Context.CommandPrefix}roles remove {role.Role.Name}` to make it inaccessible again!
                        """)
                .Build());
            },
            Preconditions: [userHasPermission.Create(GuildPermission.ManageRoles)]
        );

        var context = DiscordNetContextMapper.MapToRunContext(Context, new());
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("remove")]
    public async Task<RuntimeResult> RemoveAsync(
        [Remainder]
        RoleArgument<IRole> role
    )
    {
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            async () =>
            {
                await accessibleRoleRepository.RemoveAccessibleRoleAsync(role.Role);

                return new EmbedResult(new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithDescription(
                        $"""
                        Successfully made {role.Role.Mention} inaccessible to everyone in the server. 😊
                        This action did not remove the role from users who already had it.
                        Use `{Context.CommandPrefix}roles` to see remaining accessible roles!"
                        """)
                .Build());
            },
            Preconditions: [userHasPermission.Create(GuildPermission.ManageRoles)]
        );

        var context = DiscordNetContextMapper.MapToRunContext(Context, new());
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("group")]
    public async Task<RuntimeResult> GroupAsync(
        AccessibleGroupName group,
        [Remainder]
        RoleNotEveryoneArgument<IRole> role
    )
    {
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            async () =>
            {
                EmbedBuilder embed = new();

                if (group.Name == "clear")
                {
                    await accessibleRoleRepository.ClearGroupFromAccessibleRoleAsync(role.Role);

                    embed
                        .WithColor(TaylorBotColors.SuccessColor)
                        .WithDescription(
                            $"""
                            Successfully removed {role.Role.Mention} from its group.
                            Use `{Context.CommandPrefix}roles` to see all accessible roles.
                            """);
                }
                else
                {
                    await accessibleRoleRepository.AddOrUpdateAccessibleRoleWithGroupAsync(role.Role, group);

                    embed
                        .WithColor(TaylorBotColors.SuccessColor)
                        .WithDescription(
                            $"""
                            Successfully put {role.Role.Mention} in the '{group.Name}' group.
                            Users can only get one accessible role of the same group when using `{Context.CommandPrefix}role`.
                            Use `{Context.CommandPrefix}roles group clear {role.Role.Name}` to remove it from the group.
                            """);
                }

                return new EmbedResult(embed.Build());
            },
            Preconditions: [userHasPermission.Create(GuildPermission.ManageRoles)]
        );

        var context = DiscordNetContextMapper.MapToRunContext(Context, new());
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }
}
