using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.StringMappers;

namespace TaylorBot.Net.Commands.Preconditions
{
    public class RequireTaylorBotPermissionAttribute : PreconditionAttribute
    {
        private readonly PermissionStringMapper _permissionStringMapper = new();
        private readonly RequireBotPermissionAttribute _requireBotPermission;

        public GuildPermission GuildPermission { get; }

        public RequireTaylorBotPermissionAttribute(GuildPermission permission)
        {
            _requireBotPermission = new RequireBotPermissionAttribute(permission);
            GuildPermission = permission;
        }

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var permissionResult = await _requireBotPermission.CheckPermissionsAsync(context, command, services);
            if (permissionResult.IsSuccess)
            {
                return PreconditionResult.FromSuccess();
            }
            else
            {
                var commandName = command.Aliases.First();
                var permissionName = GuildPermission.ToString();
                var permissionUIName = _permissionStringMapper.MapGuildPermissionToString(GuildPermission);

                return TaylorBotPreconditionResult.FromUserError(
                    privateReason: $"{commandName} requires the bot to have permission {permissionName}",
                    userReason: string.Join('\n', new[] {
                        $"TaylorBot can't use `{commandName}` because I don't have the '{permissionUIName}' permission in this server.",
                        $"Add the '{permissionUIName}' permission to my role in the server settings."
                    })
                );
            }
        }
    }
}
