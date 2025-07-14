using Discord;
using Discord.Commands;
using Discord.Net;
using System.Net;
using TaylorBot.Net.Commands.Discord.Program.Modules.Jail.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.Mod.Domain;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.Core.Strings;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Jail.Commands;

[Name("Jail 👮")]
[Group("jail")]
public class JailModule(
    ICommandRunner commandRunner,
    IJailRepository jailRepository,
    IModChannelLogger modChannelLogger,
    UserHasPermissionOrOwnerPrecondition.Factory userHasPermission,
    TaylorBotHasPermissionPrecondition.Factory botHasPermission) : TaylorBotModule
{
    [Priority(-1)]
    [Command]
    public async Task<RuntimeResult> JailAsync(
        [Remainder]
        IMentionedUserNotAuthorOrClient<IGuildUser> mentionedUser
    )
    {
        var context = DiscordNetContextMapper.MapToRunContext(Context, new());
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            async () =>
            {
                EmbedBuilder embed = new();

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
                                    .WithDescription(string.Join('\n', [
                                        $"Could not give jail role {guildJailRole.Role.Mention} to {user.Mention} due to missing permissions.",
                                        $"In server settings, make sure TaylorBot's role is higher in the list than {guildJailRole.Role.Mention}."
                                    ]))
                                .Build());
                            }
                            else
                            {
                                throw;
                            }
                        }

                        var wasLogged = await modChannelLogger.TrySendModLogAsync(Context.Guild, new(Context.User), new(user), logEmbed => logEmbed
                            .WithColor(new(95, 107, 120))
                            .WithFooter("User jailed")
                        );

                        return new EmbedResult(modChannelLogger.CreateResultEmbed(context, wasLogged, $"{user.FormatTagAndMention()} was successfully jailed. 👍"));

                    default: throw new NotImplementedException();
                }
            },
            Preconditions: [
                userHasPermission.Create(GuildPermission.ModerateMembers),
                botHasPermission.Create(GuildPermission.ManageRoles),
            ]
        );

        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("free")]
    public async Task<RuntimeResult> FreeAsync(
        [Remainder]
        IMentionedUserNotAuthor<IGuildUser> mentionedUser
    )
    {
        var context = DiscordNetContextMapper.MapToRunContext(Context, new());
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            async () =>
            {
                EmbedBuilder embed = new();

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
                                    .WithDescription(string.Join('\n', [
                                        $"Could not give jail role {guildJailRole.Role.Mention} to {user.Mention} due to missing permissions.",
                                        $"In server settings, make sure TaylorBot's role is higher in the list than {guildJailRole.Role.Mention}."
                                    ]))
                                .Build());
                            }
                            else
                            {
                                throw;
                            }
                        }

                        var wasLogged = await modChannelLogger.TrySendModLogAsync(Context.Guild, new(Context.User), new(user), logEmbed => logEmbed
                            .WithColor(new(119, 136, 153))
                            .WithFooter("User freed")
                        );

                        return new EmbedResult(modChannelLogger.CreateResultEmbed(context, wasLogged, $"{user.FormatTagAndMention()} was successfully freed. 👍"));

                    default: throw new NotImplementedException();
                }
            },
            Preconditions: [
                userHasPermission.Create(GuildPermission.ModerateMembers),
                botHasPermission.Create(GuildPermission.ManageRoles),
            ]
        );

        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("set")]
    public async Task<RuntimeResult> SetAsync(
        [Remainder]
        RoleNotEveryoneArgument<IRole> role
    )
    {
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            async () =>
            {
                await jailRepository.SetJailRoleAsync(Context.Guild, role.Role);

                return new EmbedResult(new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithDescription(string.Join('\n', [
                        $"Role {role.Role.Mention} was successfully set as the jail role for this server.",
                        $"You can now use `{Context.CommandPrefix}jail @user` and they will receive {role.Role.Mention}."
                    ]))
                .Build());
            },
            Preconditions: [userHasPermission.Create(GuildPermission.ManageGuild)]
        );

        var context = DiscordNetContextMapper.MapToRunContext(Context, new());
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }


    private interface IGuildJailRoleResult { }

    private sealed record GuildJailRoleResult(IRole Role) : IGuildJailRoleResult;

    private sealed record GuildJailRoleErrorResult(string ErrorMessage) : IGuildJailRoleResult;

    private async ValueTask<IGuildJailRoleResult> GetGuildJailRoleAsync()
    {
        var jailRole = await jailRepository.GetJailRoleAsync(Context.Guild);

        if (jailRole == null)
        {
            return new GuildJailRoleErrorResult(string.Join('\n', [
                $"No jail role has been set for this server, you must set it up for this command to work.",
                $"Create a role with limited permissions in server settings and use `{Context.CommandPrefix}jail set` to set it as the jail role."
            ]));
        }

        var guildJailRole = Context.Guild.GetRole(jailRole.RoleId.Id);

        if (guildJailRole == null)
        {
            return new GuildJailRoleErrorResult(string.Join('\n', [
                "The previously set jail role could not be found. Was it deleted?",
                $"Use `{Context.CommandPrefix}jail set` to set another one."
            ]));
        }

        return new GuildJailRoleResult(guildJailRole);
    }
}
