using Discord;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.StringMappers;

namespace TaylorBot.Net.Commands.Preconditions
{
    public class TaylorBotHasPermissionPrecondition : ICommandPrecondition
    {
        private readonly PermissionStringMapper _permissionStringMapper = new();

        public GuildPermission GuildPermission { get; }

        public TaylorBotHasPermissionPrecondition(GuildPermission permission)
        {
            GuildPermission = permission;
        }

        public async ValueTask<ICommandResult> CanRunAsync(Command command, RunContext context)
        {
            var botGuildUser = await context.Guild!.GetCurrentUserAsync();
            if (botGuildUser.GuildPermissions.Has(GuildPermission))
            {
                return new PreconditionPassed();
            }
            else
            {
                var permissionName = GuildPermission.ToString();
                var permissionUIName = _permissionStringMapper.MapGuildPermissionToString(GuildPermission);

                return new PreconditionFailed(
                    PrivateReason: $"{command.Metadata.Name} requires the bot to have permission {permissionName}",
                    UserReason: new(string.Join('\n', new[] {
                        $"TaylorBot can't use `{command.Metadata.Name}` because I don't have the '{permissionUIName}' permission in this server.",
                        $"Add the '{permissionUIName}' permission to my role in the server settings."
                    }))
                );
            }
        }
    }
}
