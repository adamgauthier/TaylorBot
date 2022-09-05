using Discord;
using Discord.Commands;
using Discord.Net;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.Jail.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.Mod.Domain;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.Core.Strings;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Jail.Commands
{
    [Name("Jail 👮")]
    [Group("jail")]
    public class JailModule : TaylorBotModule
    {
        private readonly ICommandRunner _commandRunner;
        private readonly IJailRepository _jailRepository;
        private readonly IModChannelLogger _modChannelLogger;

        public JailModule(ICommandRunner commandRunner, IJailRepository jailRepository, IModChannelLogger modChannelLogger)
        {
            _commandRunner = commandRunner;
            _jailRepository = jailRepository;
            _modChannelLogger = modChannelLogger;
        }

        [Priority(-1)]
        [Command]
        [Summary("Jails a user in this server by giving them the jail role.")]
        public async Task<RuntimeResult> JailAsync(
            [Summary("What user would you like to be jailed?")]
            [Remainder]
            IMentionedUserNotAuthorOrClient<IGuildUser> mentionedUser
        )
        {
            var context = DiscordNetContextMapper.MapToRunContext(Context);
            var command = new Command(
                DiscordNetContextMapper.MapToCommandMetadata(Context),
                async () =>
                {
                    var embed = new EmbedBuilder()
                        .WithUserAsAuthor(Context.User);

                    var guildJailRoleResult = await GetGuildJailRoleAsync();

                    switch (guildJailRoleResult)
                    {
                        case GuildJailRoleErrorResult errorResult:
                            return new EmbedResult(embed
                                .WithColor(TaylorBotColors.ErrorColor)
                                .WithDescription(errorResult.ErrorMessage)
                            .Build());

                        case GuildJailRoleResult guildJailRole:
                            var user = await mentionedUser.GetTrackedUserAsync();

                            try
                            {
                                await user.AddRoleAsync(guildJailRole.Role, new() { AuditLogReason = $"{Context.User.FormatLog()} used jail" });
                            }
                            catch (HttpException e)
                            {
                                if (e.HttpCode == HttpStatusCode.Forbidden)
                                {
                                    return new EmbedResult(embed
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

                            var wasLogged = await _modChannelLogger.TrySendModLogAsync(Context.Guild, Context.User, user, logEmbed => logEmbed
                                .WithColor(new(95, 107, 120))
                                .WithFooter("User jailed")
                            );

                            return new EmbedResult(_modChannelLogger.CreateResultEmbed(context, wasLogged, $"{user.FormatTagAndMention()} was successfully jailed. 👍"));

                        default: throw new NotImplementedException();
                    }
                },
                Preconditions: new ICommandPrecondition[] {
                    new InGuildPrecondition(),
                    new UserHasPermissionOrOwnerPrecondition(GuildPermission.ModerateMembers),
                    new TaylorBotHasPermissionPrecondition(GuildPermission.ManageRoles)
                }
            );

            var result = await _commandRunner.RunAsync(command, context);

            return new TaylorBotResult(result, context);
        }

        [Command("free")]
        [Summary("Frees a previously jailed user in this server by removing the jail role.")]
        public async Task<RuntimeResult> FreeAsync(
            [Summary("What user would you like to be freed?")]
            [Remainder]
            IMentionedUserNotAuthor<IGuildUser> mentionedUser
        )
        {
            var context = DiscordNetContextMapper.MapToRunContext(Context);
            var command = new Command(
                DiscordNetContextMapper.MapToCommandMetadata(Context),
                async () =>
                {
                    var embed = new EmbedBuilder()
                        .WithUserAsAuthor(Context.User);

                    var guildJailRoleResult = await GetGuildJailRoleAsync();

                    switch (guildJailRoleResult)
                    {
                        case GuildJailRoleErrorResult errorResult:
                            return new EmbedResult(embed
                                .WithColor(TaylorBotColors.ErrorColor)
                                .WithDescription(errorResult.ErrorMessage)
                            .Build());

                        case GuildJailRoleResult guildJailRole:
                            var user = await mentionedUser.GetTrackedUserAsync();

                            if (!user.RoleIds.Contains(guildJailRole.Role.Id))
                            {
                                return new EmbedResult(embed
                                    .WithColor(TaylorBotColors.ErrorColor)
                                    .WithDescription($"{user.Mention} does not have the jail role {guildJailRole.Role.Mention}.")
                                .Build());
                            }

                            try
                            {
                                await user.RemoveRoleAsync(guildJailRole.Role, new() { AuditLogReason = $"{Context.User.FormatLog()} used jail free" });
                            }
                            catch (HttpException e)
                            {
                                if (e.HttpCode == HttpStatusCode.Forbidden)
                                {
                                    return new EmbedResult(embed
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

                            var wasLogged = await _modChannelLogger.TrySendModLogAsync(Context.Guild, Context.User, user, logEmbed => logEmbed
                                .WithColor(new(119, 136, 153))
                                .WithFooter("User freed")
                            );

                            return new EmbedResult(_modChannelLogger.CreateResultEmbed(context, wasLogged, $"{user.FormatTagAndMention()} was successfully freed. 👍"));

                        default: throw new NotImplementedException();
                    }
                },
                Preconditions: new ICommandPrecondition[] {
                    new InGuildPrecondition(),
                    new UserHasPermissionOrOwnerPrecondition(GuildPermission.ModerateMembers),
                    new TaylorBotHasPermissionPrecondition(GuildPermission.ManageRoles)
                }
            );

            var result = await _commandRunner.RunAsync(command, context);

            return new TaylorBotResult(result, context);
        }

        [Command("set")]
        [Summary("Sets the jail role for this server.")]
        public async Task<RuntimeResult> SetAsync(
            [Summary("What role would you like to set to the jail role?")]
            [Remainder]
            RoleNotEveryoneArgument<IRole> role
        )
        {
            var command = new Command(
                DiscordNetContextMapper.MapToCommandMetadata(Context),
                async () =>
                {
                    await _jailRepository.SetJailRoleAsync(Context.Guild, role.Role);

                    return new EmbedResult(new EmbedBuilder()
                        .WithUserAsAuthor(Context.User)
                        .WithColor(TaylorBotColors.SuccessColor)
                        .WithDescription(string.Join('\n', new[] {
                            $"Role {role.Role.Mention} was successfully set as the jail role for this server.",
                            $"You can now use `{Context.CommandPrefix}jail @user` and they will receive {role.Role.Mention}."
                        }))
                    .Build());
                },
                Preconditions: new ICommandPrecondition[] {
                    new InGuildPrecondition(),
                    new UserHasPermissionOrOwnerPrecondition(GuildPermission.ManageGuild)
                }
            );

            var context = DiscordNetContextMapper.MapToRunContext(Context);
            var result = await _commandRunner.RunAsync(command, context);

            return new TaylorBotResult(result, context);
        }


        private interface IGuildJailRoleResult { }

        private record GuildJailRoleResult(IRole Role) : IGuildJailRoleResult;

        private record GuildJailRoleErrorResult(string ErrorMessage) : IGuildJailRoleResult;

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
