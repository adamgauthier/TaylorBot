using Discord;
using Discord.Commands;
using Discord.Net;
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

            var jailRole = await _jailRepository.GetJailRoleAsync(Context.Guild);

            if (jailRole == null)
            {
                return new TaylorBotEmbedResult(embed
                    .WithColor(TaylorBotColors.ErrorColor)
                    .WithDescription(string.Join('\n', new[] {
                        $"No jail role has been set for this server, you must set it up for this command to work.",
                        $"Create a role with limited permissions in server settings and use `{Context.CommandPrefix}jail set` to set it as the jail role."
                    }))
                .Build());
            }

            var guildJailRole = Context.Guild.GetRole(jailRole.RoleId.Id);

            if (guildJailRole == null)
            {
                return new TaylorBotEmbedResult(embed
                    .WithColor(TaylorBotColors.ErrorColor)
                    .WithDescription(string.Join('\n', new[] {
                        "The previously set jail role could not be found. Was it deleted?",
                        $"Use `{Context.CommandPrefix}jail set` to set another one."
                    }))
                .Build());
            }

            var user = await mentionedUser.GetTrackedUserAsync();

            try
            {
                await user.AddRoleAsync(guildJailRole);
            }
            catch (HttpException e)
            {
                if (e.HttpCode == HttpStatusCode.Forbidden)
                {
                    return new TaylorBotEmbedResult(embed
                        .WithColor(TaylorBotColors.ErrorColor)
                        .WithDescription(string.Join('\n', new[] {
                            $"Could not give jail role {guildJailRole.Mention} to {user.Mention} due to missing permissions.",
                            $"In server settings, make sure TaylorBot's role is higher in the list than {guildJailRole.Mention}."
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
    }
}
