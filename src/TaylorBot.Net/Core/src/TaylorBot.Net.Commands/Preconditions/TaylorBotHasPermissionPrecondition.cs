using Discord;
using TaylorBot.Net.Commands.StringMappers;

namespace TaylorBot.Net.Commands.Preconditions;

public class TaylorBotHasPermissionPrecondition(GuildPermission permission) : ICommandPrecondition
{
    private readonly PermissionStringMapper _permissionStringMapper = new();

    public GuildPermission GuildPermission { get; } = permission;

    public async ValueTask<ICommandResult> CanRunAsync(Command command, RunContext context)
    {
        var guild = context.Guild ?? throw new NotImplementedException();
        var botGuildUser = await guild.GetCurrentUserAsync();
        if (botGuildUser.GuildPermissions.Has(GuildPermission) || botGuildUser.Guild.OwnerId == botGuildUser.Id)
        {
            return new PreconditionPassed();
        }
        else
        {
            var permissionName = GuildPermission.ToString();
            var permissionUIName = _permissionStringMapper.MapGuildPermissionToString(GuildPermission);

            return new PreconditionFailed(
                PrivateReason: $"{command.Metadata.Name} requires the bot to have permission {permissionName}",
                UserReason: new(
                    $"""
                    TaylorBot can't use `{command.Metadata.Name}` because it needs the **{permissionUIName}** permission in this server.
                    Add the '{permissionUIName}' permission to the TaylorBot role in server settings.
                    """)
            );
        }
    }
}
