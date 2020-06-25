using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.StringMappers;

namespace TaylorBot.Net.Commands.Preconditions
{
    public class RequireUserPermissionOrOwnerAttribute : PreconditionAttribute
    {
        private readonly PermissionStringMapper _permissionStringMapper = new PermissionStringMapper();
        private readonly RequireOwnerAttribute _requireOwner = new RequireOwnerAttribute();
        private readonly RequireUserPermissionAttribute _requireUserPermission;

        public GuildPermission GuildPermission { get; }

        public RequireUserPermissionOrOwnerAttribute(GuildPermission permission)
        {
            _requireUserPermission = new RequireUserPermissionAttribute(permission);
            GuildPermission = permission;
        }

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var permissionResult = await _requireUserPermission.CheckPermissionsAsync(context, command, services);
            if (permissionResult.IsSuccess)
            {
                return PreconditionResult.FromSuccess();
            }
            else
            {
                var ownerResult = await _requireOwner.CheckPermissionsAsync(context, command, services);
                if (ownerResult.IsSuccess)
                {
                    return PreconditionResult.FromSuccess();
                }
                else
                {
                    var commandName = command.Aliases.First();
                    var permissionName = GuildPermission.ToString();
                    var permissionUIName = _permissionStringMapper.MapGuildPermissionToString(GuildPermission);

                    return TaylorBotPreconditionResult.FromUserError(
                        privateReason: $"{commandName} can only be used with permission {permissionName}",
                        userReason: string.Join('\n', new[] {
                            $"You can't use `{commandName}` because you don't have the '{permissionUIName}' permission in this server.",
                            "Ask someone with more permissions than you to use the command or to give you this permission in the server settings."
                        })
                    );
                }
            }
        }
    }
}
