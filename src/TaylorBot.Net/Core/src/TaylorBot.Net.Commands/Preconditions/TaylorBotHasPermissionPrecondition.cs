using Discord;
using Microsoft.Extensions.DependencyInjection;
using TaylorBot.Net.Commands.StringMappers;

namespace TaylorBot.Net.Commands.Preconditions;

public class TaylorBotHasPermissionPrecondition(InGuildPrecondition.Factory inGuild, GuildPermission permission) : ICommandPrecondition
{
    public class Factory(IServiceProvider services)
    {
        public TaylorBotHasPermissionPrecondition Create(GuildPermission permission) =>
            ActivatorUtilities.CreateInstance<TaylorBotHasPermissionPrecondition>(services, permission);
    }

    private readonly PermissionStringMapper _permissionStringMapper = new();
    private readonly InGuildPrecondition _inGuild = inGuild.Create(botMustBeInGuild: true);

    public GuildPermission GuildPermission { get; } = permission;

    public async ValueTask<ICommandResult> CanRunAsync(Command command, RunContext context)
    {
        if (context.Guild?.Fetched == null)
        {
            return await _inGuild.CanRunAsync(command, context);
        }

        var botGuildUser = await context.Guild.Fetched.GetCurrentUserAsync();
        if (botGuildUser.GuildPermissions.Has(GuildPermission) || botGuildUser.Guild.OwnerId == botGuildUser.Id)
        {
            return new PreconditionPassed();
        }
        else
        {
            var permissionUIName = _permissionStringMapper.MapGuildPermissionToString(GuildPermission);

            return new PreconditionFailed(
                PrivateReason: $"{command.Metadata.Name} requires the bot to have permission {GuildPermission}",
                UserReason: new(
                    $"""
                    TaylorBot can't use `{command.Metadata.Name}` because it needs the **{permissionUIName}** permission in this server.
                    Add the '{permissionUIName}' permission to the TaylorBot role in server settings.
                    """)
            );
        }
    }
}
