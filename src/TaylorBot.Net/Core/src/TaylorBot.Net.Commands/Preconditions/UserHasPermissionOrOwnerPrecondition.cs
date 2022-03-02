using Discord;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.StringMappers;

namespace TaylorBot.Net.Commands.Preconditions
{
    public class UserHasPermissionOrOwnerPrecondition : ICommandPrecondition
    {
        private readonly PermissionStringMapper _permissionStringMapper = new();
        private readonly TaylorBotOwnerPrecondition _taylorBotOwnerPrecondition = new();

        public GuildPermission GuildPermission { get; }

        public UserHasPermissionOrOwnerPrecondition(GuildPermission permission)
        {
            GuildPermission = permission;
        }

        public async ValueTask<ICommandResult> CanRunAsync(Command command, RunContext context)
        {
            var guildUser = (IGuildUser)context.User;
            if (guildUser.GuildPermissions.Has(GuildPermission) || guildUser.Guild.OwnerId == guildUser.Id)
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
                    var permissionName = GuildPermission.ToString();
                    var permissionUIName = _permissionStringMapper.MapGuildPermissionToString(GuildPermission);

                    return new PreconditionFailed(
                        PrivateReason: $"{command.Metadata.Name} can only be used with permission {permissionName}",
                        UserReason: new(string.Join('\n', new[] {
                            $"You can't use `{command.Metadata.Name}` because you don't have the '{permissionUIName}' permission in this server.",
                            "Ask someone with more permissions than you to use the command or to give you this permission in the server settings."
                        }))
                    );
                }
            }
        }
    }
}
