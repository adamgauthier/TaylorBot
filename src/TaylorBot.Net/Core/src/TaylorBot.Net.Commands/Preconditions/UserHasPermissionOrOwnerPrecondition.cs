using Discord;
using Microsoft.Extensions.DependencyInjection;
using TaylorBot.Net.Commands.StringMappers;

namespace TaylorBot.Net.Commands.Preconditions;

public class UserHasPermissionOrOwnerPrecondition(InGuildPrecondition.Factory inGuild, TaylorBotOwnerPrecondition taylorBotOwner, PermissionStringMapper permissionMapper, params GuildPermission[] permissions) : ICommandPrecondition
{
    public class Factory(IServiceProvider services)
    {
        public UserHasPermissionOrOwnerPrecondition Create(params GuildPermission[] permissions) =>
            ActivatorUtilities.CreateInstance<UserHasPermissionOrOwnerPrecondition>(services, permissions);
    }

    public async ValueTask<ICommandResult> CanRunAsync(Command command, RunContext context)
    {
        if (context.Guild == null)
        {
            return await inGuild.Create().CanRunAsync(command, context);
        }

        if (context.User.MemberInfo == null)
        {
            return new PreconditionFailed(
                PrivateReason: $"{command.Metadata.Name} can only be used by a guild member",
                UserReason: new($"You can't use {context.MentionCommand(command)} because it can only be used in a server 🚫")
            );
        }

        if (context.Guild.Fetched?.OwnerId == context.User.Id)
        {
            return new PreconditionPassed();
        }
        else
        {
            if (permissions.Any(context.User.MemberInfo.Permissions.Has))
            {
                return new PreconditionPassed();
            }
            else
            {
                if (await taylorBotOwner.CanRunAsync(command, context) is PreconditionPassed)
                {
                    return new PreconditionPassed();
                }
                else
                {
                    var permissionMessage = permissions.Length > 1
                        ? $"one of these permissions in this server: **{string.Join("** or **", permissions.Select(permissionMapper.MapGuildPermissionToString))}**"
                        : $"the **{permissionMapper.MapGuildPermissionToString(permissions[0])}** permission in this server";

                    return new PreconditionFailed(
                        PrivateReason: $"{command.Metadata.Name} can only be used with one of {string.Join(',', permissions)}",
                        UserReason: new(
                            $"""
                            You can't use {context.MentionCommand(command)} because you need {permissionMessage} 🚫
                            Ask someone with more permissions than you to use the command or to give you this permission in the server settings ⚙️
                            """)
                    );
                }
            }
        }
    }
}
