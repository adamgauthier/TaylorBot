using Discord;
using TaylorBot.Net.Commands.StringMappers;

namespace TaylorBot.Net.Commands.Preconditions;

public class UserHasPermissionOrOwnerPrecondition(params GuildPermission[] permissions) : ICommandPrecondition
{
    private readonly PermissionStringMapper _permissionStringMapper = new();
    private readonly TaylorBotOwnerPrecondition _taylorBotOwnerPrecondition = new();
    private readonly InGuildPrecondition _inGuild = new();

    public GuildPermission[] GuildPermissions { get; } = permissions;

    public async ValueTask<ICommandResult> CanRunAsync(Command command, RunContext context)
    {
        if (context.Guild == null)
        {
            return await _inGuild.CanRunAsync(command, context);
        }

        if (context.User.MemberInfo == null)
        {
            return new PreconditionFailed(
                PrivateReason: $"{command.Metadata.Name} can only be used by a guild member",
                UserReason: new($"You can't use `{command.Metadata.Name}` because it can only be used in a server.")
            );
        }

        if (context.Guild.Fetched?.OwnerId == context.User.Id)
        {
            return new PreconditionPassed();
        }
        else
        {
            if (GuildPermissions.Any(context.User.MemberInfo.Permissions.Has))
            {
                return new PreconditionPassed();
            }
            else
            {
                if (await _taylorBotOwnerPrecondition.CanRunAsync(command, context) is PreconditionPassed)
                {
                    return new PreconditionPassed();
                }
                else
                {
                    var permissionMessage = GuildPermissions.Length > 1
                        ? $"one of these permissions in this server: **{string.Join("** or **", GuildPermissions.Select(_permissionStringMapper.MapGuildPermissionToString))}**"
                        : $"the **{_permissionStringMapper.MapGuildPermissionToString(GuildPermissions[0])}** permission in this server";

                    return new PreconditionFailed(
                        PrivateReason: $"{command.Metadata.Name} can only be used with one of {string.Join(',', GuildPermissions)}",
                        UserReason: new(
                            $"""
                            You can't use `{command.Metadata.Name}` because you need {permissionMessage}.
                            Ask someone with more permissions than you to use the command or to give you this permission in the server settings.
                            """)
                    );
                }
            }
        }
    }
}
