using Discord;
using Discord.Commands;
using Humanizer;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.AccessibleRoles.Domain;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules
{
    [RequireInGuild]
    [Name("Roles 🆔")]
    [Group("roles")]
    [Alias("role", "gr")]
    public class AccessibleRolesModule : TaylorBotModule
    {
        private readonly IAccessibleRoleRepository _accessibleRoleRepository;

        public AccessibleRolesModule(IAccessibleRoleRepository accessibleRoleRepository)
        {
            _accessibleRoleRepository = accessibleRoleRepository;
        }

        [Priority(-1)]
        [RequireTaylorBotPermission(GuildPermission.ManageRoles)]
        [Command]
        [Summary("Assigns you a role that set to accessible in this server.")]
        public async Task<RuntimeResult> GetAsync(
            [Summary("What role would you like to get?")]
            [Remainder]
            RoleNotEveryoneArgument<IRole>? role = null
        )
        {
            var member = (IGuildUser)Context.User;
            var embed = new EmbedBuilder().WithUserAsAuthor(member);

            if (role != null)
            {
                if (!member.RoleIds.Contains(role.Role.Id))
                {
                    if (await _accessibleRoleRepository.IsRoleAccessibleAsync(role.Role))
                    {
                        await member.AddRoleAsync(role.Role, new RequestOptions
                        {
                            AuditLogReason = $"Assigned accessible role on user's request with message id {Context.Message.Id}."
                        });

                        embed
                            .WithColor(TaylorBotColors.SuccessColor)
                            .WithDescription(string.Join('\n', new[] {
                                $"You now have {role.Role.Mention}. 😊",
                                $"Use `{Context.CommandPrefix}dr {role.Role.Name}` to drop it!"
                            }));
                    }
                    else
                    {
                        embed
                            .WithColor(TaylorBotColors.ErrorColor)
                            .WithDescription(string.Join('\n', new[] {
                                $"Sorry, {role.Role.Mention} is not marked as accessible so I can't give it to you.",
                                $"Use `{Context.CommandPrefix}aar {role.Role.Name}` to make it accessible to everyone!"
                            }));
                    }
                }
                else
                {
                    embed
                        .WithColor(TaylorBotColors.ErrorColor)
                        .WithDescription(string.Join('\n', new[] {
                            $"You already have role {role.Role.Mention}.",
                            $"Use `{Context.CommandPrefix}dr {role.Role.Name}` to drop it!"
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
                    embed
                        .WithDescription(string.Join('\n', new[] {
                            $"Here are the accessible roles in this server, use `{Context.CommandPrefix}role role-name` to get one of them.",
                            string.Join(", ", accessibleRoles.Select(r => MentionUtils.MentionRole(r.RoleId.Id)))
                        }).Truncate(2048));
                }
                else
                {
                    embed.WithDescription(string.Join('\n', new[] {
                        $"There is currently no accessible role in this server.",
                        $"Accessible roles are roles that everyone has access to using `{Context.CommandPrefix}role`.",
                        $"Use `{Context.CommandPrefix}aar role-name` to add one!"
                    }));
                }

            }

            return new TaylorBotEmbedResult(embed.Build());
        }
    }
}
