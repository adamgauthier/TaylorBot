using Discord;
using TaylorBot.Net.Commands.StringMappers;

namespace TaylorBot.Net.Commands.Preconditions;

public class UserHasPermissionOrOwnerPrecondition : ICommandPrecondition
{
    private readonly PermissionStringMapper _permissionStringMapper = new();
    private readonly TaylorBotOwnerPrecondition _taylorBotOwnerPrecondition = new();

    public GuildPermission[] GuildPermissions { get; }

    public UserHasPermissionOrOwnerPrecondition(params GuildPermission[] permissions)
    {
        GuildPermissions = permissions;
    }

    public async ValueTask<ICommandResult> CanRunAsync(Command command, RunContext context)
    {
        var guildUser = (IGuildUser)context.User;
        if (guildUser.Guild.OwnerId == guildUser.Id || GuildPermissions.Any(guildUser.GuildPermissions.Has))
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
                    ? $"one of these permissions in this server: **{string.Join("** or **", GuildPermissions.Select(p => _permissionStringMapper.MapGuildPermissionToString(p)))}**"
                    : $"the **{_permissionStringMapper.MapGuildPermissionToString(GuildPermissions[0])}** permission in this server";

                return new PreconditionFailed(
                    PrivateReason: $"{command.Metadata.Name} can only be used with one of {string.Join(',', GuildPermissions)}",
                    UserReason: new(string.Join('\n', new[] {
                        $"You can't use `{command.Metadata.Name}` because you need {permissionMessage}.",
                        "Ask someone with more permissions than you to use the command or to give you this permission in the server settings."
                    }))
                );
            }
        }
    }
}
