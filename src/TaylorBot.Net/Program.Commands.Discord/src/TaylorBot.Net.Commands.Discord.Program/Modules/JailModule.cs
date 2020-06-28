using Discord;
using Discord.Commands;
using Discord.Net;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Jail.Domain;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules
{
    [RequireInGuild]
    [Name("Jail 👮")]
    [Group("jail")]
    public class JailModule : TaylorBotModule
    {
        private readonly IJailRepository _jailRepository;

        public JailModule(IJailRepository jailRepository)
        {
            _jailRepository = jailRepository;
        }

        [Priority(-1)]
        [RequireUserPermissionOrOwner(GuildPermission.KickMembers)]
        [RequireTaylorBotPermission(GuildPermission.ManageRoles)]
        [Command]
        [Summary("Jails a user in this server by giving them the jail role.")]
        public async Task<RuntimeResult> JailAsync(
            [Summary("What user would you like to be jailed?")]
            [Remainder]
            IMentionedUserNotAuthor<IGuildUser> mentionedUser
        )
        {
            var embed = new EmbedBuilder()
                .WithUserAsAuthor(Context.User);

            var guildJailRoleResult = await GetGuildJailRoleAsync();

            switch (guildJailRoleResult)
            {
                case GuildJailRoleErrorResult errorResult:
                    return new TaylorBotEmbedResult(embed
                        .WithColor(TaylorBotColors.ErrorColor)
                        .WithDescription(errorResult.ErrorMessage)
                    .Build());

                case GuildJailRoleResult guildJailRole:
                    var user = await mentionedUser.GetTrackedUserAsync();

                    try
                    {
                        await user.AddRoleAsync(guildJailRole.Role);
                    }
                    catch (HttpException e)
                    {
                        if (e.HttpCode == HttpStatusCode.Forbidden)
                        {
                            return new TaylorBotEmbedResult(embed
                                .WithColor(TaylorBotColors.ErrorColor)
                                .WithDescription(string.Join('\n', new[] {
                                    $"Could not give jail role {guildJailRole.Role.Mention} to {user.Mention} due to missing permissions.",
                                    $"In server settings, make sure TaylorBot's role is higher in the list than {guildJailRole.Role.Mention}."
                                }))
                            .Build());
                        }
                        else
                        {
                            throw;
                        }
                    }

                    return new TaylorBotEmbedResult(embed
                        .WithColor(TaylorBotColors.SuccessColor)
                        .WithDescription($"{user.Mention} was successfully jailed.")
                    .Build());

                default: throw new NotImplementedException();
            }
        }

        [RequireUserPermissionOrOwner(GuildPermission.KickMembers)]
        [RequireTaylorBotPermission(GuildPermission.ManageRoles)]
        [Command("free")]
        [Summary("Frees a previously jailed user in this server by removing the jail role.")]
        public async Task<RuntimeResult> FreeAsync(
            [Summary("What user would you like to be freed?")]
            [Remainder]
            IMentionedUserNotAuthor<IGuildUser> mentionedUser
        )
        {
            var embed = new EmbedBuilder()
                .WithUserAsAuthor(Context.User);

            var guildJailRoleResult = await GetGuildJailRoleAsync();

            switch (guildJailRoleResult)
            {
                case GuildJailRoleErrorResult errorResult:
                    return new TaylorBotEmbedResult(embed
                        .WithColor(TaylorBotColors.ErrorColor)
                        .WithDescription(errorResult.ErrorMessage)
                    .Build());

                case GuildJailRoleResult guildJailRole:
                    var user = await mentionedUser.GetTrackedUserAsync();

                    if (!user.RoleIds.Contains(guildJailRole.Role.Id))
                    {
                        return new TaylorBotEmbedResult(embed
                            .WithColor(TaylorBotColors.ErrorColor)
                            .WithDescription($"{user.Mention} does not have the jail role {guildJailRole.Role.Mention}.")
                        .Build());
                    }

                    try
                    {
                        await user.RemoveRoleAsync(guildJailRole.Role);
                    }
                    catch (HttpException e)
                    {
                        if (e.HttpCode == HttpStatusCode.Forbidden)
                        {
                            return new TaylorBotEmbedResult(embed
                                .WithColor(TaylorBotColors.ErrorColor)
                                .WithDescription(string.Join('\n', new[] {
                                    $"Could not give jail role {guildJailRole.Role.Mention} to {user.Mention} due to missing permissions.",
                                    $"In server settings, make sure TaylorBot's role is higher in the list than {guildJailRole.Role.Mention}."
                                }))
                            .Build());
                        }
                        else
                        {
                            throw;
                        }
                    }

                    return new TaylorBotEmbedResult(embed
                        .WithColor(TaylorBotColors.SuccessColor)
                        .WithDescription($"{user.Mention} was successfully freed.")
                    .Build());

                default: throw new NotImplementedException();
            }
        }

        [RequireUserPermissionOrOwner(GuildPermission.ManageGuild)]
        [Command("set")]
        [Summary("Sets the jail role for this server.")]
        public async Task<RuntimeResult> SetAsync(
            [Summary("What role would you like to set to the jail role?")]
            [Remainder]
            IRole role
        )
        {
            await _jailRepository.SetJailRoleAsync(Context.Guild, role);

            return new TaylorBotEmbedResult(new EmbedBuilder()
                .WithUserAsAuthor(Context.User)
                .WithColor(TaylorBotColors.SuccessColor)
                .WithDescription(string.Join('\n', new[] {
                    $"Role {role.Mention} was successfully set as the jail role for this server.",
                    $"You can now use `{Context.CommandPrefix}jail @user` and they will receive {role.Mention}."
                }))
            .Build());
        }


        private interface IGuildJailRoleResult { }

        private class GuildJailRoleResult : IGuildJailRoleResult
        {
            public IRole Role { get; }
            public GuildJailRoleResult(IRole role) => Role = role;
        }

        private class GuildJailRoleErrorResult : IGuildJailRoleResult
        {
            public string ErrorMessage { get; }
            public GuildJailRoleErrorResult(string errorMessage) => ErrorMessage = errorMessage;
        }

        private async ValueTask<IGuildJailRoleResult> GetGuildJailRoleAsync()
        {
            var jailRole = await _jailRepository.GetJailRoleAsync(Context.Guild);

            if (jailRole == null)
            {
                return new GuildJailRoleErrorResult(string.Join('\n', new[] {
                    $"No jail role has been set for this server, you must set it up for this command to work.",
                    $"Create a role with limited permissions in server settings and use `{Context.CommandPrefix}jail set` to set it as the jail role."
                }));
            }

            var guildJailRole = Context.Guild.GetRole(jailRole.RoleId.Id);

            if (guildJailRole == null)
            {
                return new GuildJailRoleErrorResult(string.Join('\n', new[] {
                    "The previously set jail role could not be found. Was it deleted?",
                    $"Use `{Context.CommandPrefix}jail set` to set another one."
                }));
            }

            return new GuildJailRoleResult(guildJailRole);
        }
    }
}
