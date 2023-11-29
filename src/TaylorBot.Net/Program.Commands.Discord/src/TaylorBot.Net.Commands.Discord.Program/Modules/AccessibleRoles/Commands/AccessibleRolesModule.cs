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
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.AccessibleRoles.Commands
{
    [Name("Roles 🆔")]
    [Group("roles")]
    [Alias("role", "gr")]
    public class AccessibleRolesModule : TaylorBotModule
    {
        private readonly ICommandRunner _commandRunner;
        private readonly IAccessibleRoleRepository _accessibleRoleRepository;

        public AccessibleRolesModule(ICommandRunner commandRunner, IAccessibleRoleRepository accessibleRoleRepository)
        {
            _commandRunner = commandRunner;
            _accessibleRoleRepository = accessibleRoleRepository;
        }

        [Priority(-1)]
        [Command]
        [Summary("Assigns you a role that set to accessible in this server.")]
        public async Task<RuntimeResult> GetAsync(
            [Summary("What role would you like to get?")]
            [Remainder]
            RoleNotEveryoneArgument<IRole>? role = null
        )
        {
            var command = new Command(
                DiscordNetContextMapper.MapToCommandMetadata(Context),
                async () =>
                {
                    var member = (IGuildUser)Context.User;
                    var embed = new EmbedBuilder().WithUserAsAuthor(member);

                    if (role != null)
                    {
                        if (!member.RoleIds.Contains(role.Role.Id))
                        {
                            var accessibleRole = await _accessibleRoleRepository.GetAccessibleRoleAsync(role.Role);
                            if (accessibleRole != null)
                            {
                                var groupInfo = accessibleRole.Group != null ?
                                    new
                                    {
                                        accessibleRole.Group,
                                        MemberRolesInSameGroup = member.RoleIds.Intersect(accessibleRole.Group.OtherRoles.Select(r => r.Id)).ToList()
                                    } :
                                    null;

                                if (groupInfo != null && groupInfo.MemberRolesInSameGroup.Any())
                                {
                                    embed
                                        .WithColor(TaylorBotColors.ErrorColor)
                                        .WithDescription(string.Join('\n', new[] {
                                            $"Sorry, {role.Role.Mention} is part of the '{groupInfo.Group.Name}' group.",
                                            $"You already have {MentionUtils.MentionRole(groupInfo.MemberRolesInSameGroup.First())} which is part of the same group.",
                                            $"Use `{Context.CommandPrefix}role drop` to drop it!"
                                        }));
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
                                            .WithDescription(string.Join('\n', new[] {
                                                $"You now have {role.Role.Mention}. 😊",
                                                $"Use `{Context.CommandPrefix}role drop {role.Role.Name}` to drop it!"
                                            }));
                                    }
                                    catch (HttpException exception) when (exception.HttpCode == HttpStatusCode.Forbidden)
                                    {
                                        embed
                                            .WithColor(TaylorBotColors.ErrorColor)
                                            .WithDescription(string.Join('\n', new[] {
                                                $"Sorry, Discord does not allow me to give you {role.Role.Mention}.",
                                                $"Make sure my bot role is before {role.Role.Mention} in the roles order under server settings!"
                                            }));
                                    }
                                }
                            }
                            else
                            {
                                embed
                                    .WithColor(TaylorBotColors.ErrorColor)
                                    .WithDescription(string.Join('\n', new[] {
                                        $"Sorry, {role.Role.Mention} is not marked as accessible so I can't give it to you.",
                                        $"Use `{Context.CommandPrefix}roles add {role.Role.Name}` to make it accessible to everyone!"
                                    }));
                            }
                        }
                        else
                        {
                            embed
                                .WithColor(TaylorBotColors.ErrorColor)
                                .WithDescription(string.Join('\n', new[] {
                                    $"You already have role {role.Role.Mention}.",
                                    $"Use `{Context.CommandPrefix}role drop {role.Role.Name}` to drop it!"
                                }));
                        }
                    }
                    else
                    {
                        var accessibleRoles = (await _accessibleRoleRepository.GetAccessibleRolesAsync(Context.Guild))
                            .Where(ar => Context.Guild.Roles.Any(r => r.Id == ar.RoleId.Id))
                            .ToList();

                        embed.WithColor(TaylorBotColors.SuccessColor);

                        if (accessibleRoles.Any())
                        {
                            var ungrouped = accessibleRoles.Where(ar => ar.GroupName == null).ToList();
                            var grouped = accessibleRoles.Where(ar => ar.GroupName != null).GroupBy(ar => ar.GroupName);

                            embed.WithDescription($"Here are the accessible roles in this server, use `{Context.CommandPrefix}role role-name` to get one of them.");

                            if (ungrouped.Any())
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
                            embed.WithDescription(string.Join('\n', new[] {
                                $"There is currently no accessible role in this server.",
                                $"Accessible roles are roles that everyone has access to using `{Context.CommandPrefix}role`.",
                                $"Use `{Context.CommandPrefix}roles add role-name` to add one!"
                            }));
                        }

                    }

                    return new EmbedResult(embed.Build());
                },
                Preconditions: new ICommandPrecondition[] {
                    new InGuildPrecondition(),
                    new TaylorBotHasPermissionPrecondition(GuildPermission.ManageRoles)
                }
            );

            var context = DiscordNetContextMapper.MapToRunContext(Context);
            var result = await _commandRunner.RunAsync(command, context);

            return new TaylorBotResult(result, context);
        }

        [Command("drop")]
        [Summary("Removes an accessible role you currently have.")]
        public async Task<RuntimeResult> DropAsync(
            [Summary("What role would you like to be removed?")]
            [Remainder]
            RoleNotEveryoneArgument<IRole> role
        )
        {
            var command = new Command(
                DiscordNetContextMapper.MapToCommandMetadata(Context),
                async () =>
                {
                    var member = (IGuildUser)Context.User;
                    var embed = new EmbedBuilder().WithUserAsAuthor(member);

                    if (member.RoleIds.Contains(role.Role.Id))
                    {
                        if (await _accessibleRoleRepository.IsRoleAccessibleAsync(role.Role))
                        {
                            await member.RemoveRoleAsync(role.Role, new RequestOptions
                            {
                                AuditLogReason = $"Removed accessible role on user's request with message id {Context.Message.Id}."
                            });

                            embed
                                .WithColor(TaylorBotColors.SuccessColor)
                                .WithDescription(string.Join('\n', new[] {
                                    $"Removed {role.Role.Mention} from your roles. 😊",
                                    $"Use `{Context.CommandPrefix}role {role.Role.Name}` to get it back!"
                                }));
                        }
                        else
                        {
                            embed
                                .WithColor(TaylorBotColors.ErrorColor)
                                .WithDescription(string.Join('\n', new[] {
                                    $"Sorry, {role.Role.Mention} is not accessible so you can't drop it.",
                                    $"Use `{Context.CommandPrefix}roles add {role.Role.Name}` to make it accessible to everyone!"
                                }));
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
                Preconditions: new ICommandPrecondition[] {
                    new InGuildPrecondition(),
                    new TaylorBotHasPermissionPrecondition(GuildPermission.ManageRoles)
                }
            );

            var context = DiscordNetContextMapper.MapToRunContext(Context);
            var result = await _commandRunner.RunAsync(command, context);

            return new TaylorBotResult(result, context);
        }

        [Command("add")]
        [Summary("Adds a role as accessible to everyone in this server.")]
        public async Task<RuntimeResult> AddAsync(
            [Summary("What role would you like to make accessible?")]
            [Remainder]
            RoleNotEveryoneArgument<IRole> role
        )
        {
            var command = new Command(
                DiscordNetContextMapper.MapToCommandMetadata(Context),
                async () =>
                {
                    await _accessibleRoleRepository.AddAccessibleRoleAsync(role.Role);

                    return new EmbedResult(new EmbedBuilder()
                        .WithUserAsAuthor(Context.User)
                        .WithColor(TaylorBotColors.SuccessColor)
                        .WithDescription(string.Join('\n', new[] {
                            $"Successfully made {role.Role.Mention} accessible to everyone in the server. 😊",
                            $"Use `{Context.CommandPrefix}role {role.Role.Name}` to get it!",
                            $"Use `{Context.CommandPrefix}roles remove {role.Role.Name}` to make it inaccessible again!"
                        }))
                    .Build());
                },
                Preconditions: new ICommandPrecondition[] {
                    new InGuildPrecondition(),
                    new UserHasPermissionOrOwnerPrecondition(GuildPermission.ManageRoles)
                }
            );

            var context = DiscordNetContextMapper.MapToRunContext(Context);
            var result = await _commandRunner.RunAsync(command, context);

            return new TaylorBotResult(result, context);
        }

        [Command("remove")]
        [Summary("Removes a previously accessible role.")]
        public async Task<RuntimeResult> RemoveAsync(
            [Summary("What role would you like to make inaccessible?")]
            [Remainder]
            RoleArgument<IRole> role
        )
        {
            var command = new Command(
                DiscordNetContextMapper.MapToCommandMetadata(Context),
                async () =>
                {
                    await _accessibleRoleRepository.RemoveAccessibleRoleAsync(role.Role);

                    return new EmbedResult(new EmbedBuilder()
                        .WithUserAsAuthor(Context.User)
                        .WithColor(TaylorBotColors.SuccessColor)
                        .WithDescription(string.Join('\n', new[] {
                            $"Successfully made {role.Role.Mention} inaccessible to everyone in the server. 😊",
                            $"This action did not remove the role from users who already had it.",
                            $"Use `{Context.CommandPrefix}roles` to see remaining accessible roles!"
                        }))
                    .Build());
                },
                Preconditions: new ICommandPrecondition[] {
                    new InGuildPrecondition(),
                    new UserHasPermissionOrOwnerPrecondition(GuildPermission.ManageRoles)
                }
            );

            var context = DiscordNetContextMapper.MapToRunContext(Context);
            var result = await _commandRunner.RunAsync(command, context);

            return new TaylorBotResult(result, context);
        }

        [Command("group")]
        [Summary("Adds an accessible role to a group. Users can only get one accessible role of the same group. Use clear as group name to remove a role from its group.")]
        public async Task<RuntimeResult> GroupAsync(
            [Summary("What group would you like to add an accessible role to?")]
            AccessibleGroupName group,
            [Summary("What role would you like to make accessible in the group?")]
            [Remainder]
            RoleNotEveryoneArgument<IRole> role
        )
        {
            var command = new Command(
                DiscordNetContextMapper.MapToCommandMetadata(Context),
                async () =>
                {
                    var embed = new EmbedBuilder().WithUserAsAuthor(Context.User);

                    if (group.Name == "clear")
                    {
                        await _accessibleRoleRepository.ClearGroupFromAccessibleRoleAsync(role.Role);

                        embed
                            .WithColor(TaylorBotColors.SuccessColor)
                            .WithDescription(string.Join('\n', new[] {
                                $"Successfully removed {role.Role.Mention} from its group.",
                                $"Use `{Context.CommandPrefix}roles` to see all accessible roles."
                            }));
                    }
                    else
                    {
                        await _accessibleRoleRepository.AddOrUpdateAccessibleRoleWithGroupAsync(role.Role, group);

                        embed
                            .WithColor(TaylorBotColors.SuccessColor)
                            .WithDescription(string.Join('\n', new[] {
                                $"Successfully put {role.Role.Mention} in the '{group.Name}' group.",
                                $"Users can only get one accessible role of the same group when using `{Context.CommandPrefix}role`.",
                                $"Use `{Context.CommandPrefix}roles group clear {role.Role.Name}` to remove it from the group."
                            }));
                    }

                    return new EmbedResult(embed.Build());
                },
                Preconditions: new ICommandPrecondition[] {
                    new InGuildPrecondition(),
                    new UserHasPermissionOrOwnerPrecondition(GuildPermission.ManageRoles)
                }
            );

            var context = DiscordNetContextMapper.MapToRunContext(Context);
            var result = await _commandRunner.RunAsync(command, context);

            return new TaylorBotResult(result, context);
        }
    }
}
