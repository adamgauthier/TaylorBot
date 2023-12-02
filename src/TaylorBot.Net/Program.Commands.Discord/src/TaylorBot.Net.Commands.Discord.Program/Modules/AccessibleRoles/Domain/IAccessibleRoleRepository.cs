using Discord;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.AccessibleRoles.Domain;

public record AccessibleRoleGroup(string Name, IReadOnlyCollection<SnowflakeId> OtherRoles);

public record AccessibleRoleWithGroup(AccessibleRoleGroup? Group);

public record AccessibleRole(SnowflakeId RoleId, string? GroupName);

public record AccessibleGroupName(string Name);

public interface IAccessibleRoleRepository
{
    ValueTask<bool> IsRoleAccessibleAsync(IRole role);
    ValueTask<AccessibleRoleWithGroup?> GetAccessibleRoleAsync(IRole role);
    ValueTask<IReadOnlyCollection<AccessibleRole>> GetAccessibleRolesAsync(IGuild guild);
    ValueTask AddAccessibleRoleAsync(IRole role);
    ValueTask AddOrUpdateAccessibleRoleWithGroupAsync(IRole role, AccessibleGroupName groupName);
    ValueTask RemoveAccessibleRoleAsync(IRole role);
    ValueTask ClearGroupFromAccessibleRoleAsync(IRole role);
}
