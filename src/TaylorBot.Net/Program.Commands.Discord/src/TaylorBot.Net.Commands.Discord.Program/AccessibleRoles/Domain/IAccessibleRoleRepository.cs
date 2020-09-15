using Discord;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Commands.Discord.Program.AccessibleRoles.Domain
{
    public class AccessibleRoleGroup
    {
        public string Name { get; }
        public IReadOnlyCollection<SnowflakeId> OtherRoles { get; }

        public AccessibleRoleGroup(string name, IReadOnlyCollection<SnowflakeId> otherRolesInSameGroup)
        {
            Name = name;
            OtherRoles = otherRolesInSameGroup;
        }
    }

    public class AccessibleRoleWithGroup
    {
        public AccessibleRoleGroup? Group { get; }

        public AccessibleRoleWithGroup(AccessibleRoleGroup? group)
        {
            Group = group;
        }
    }

    public class AccessibleRole
    {
        public SnowflakeId RoleId { get; }
        public string? GroupName { get; }

        public AccessibleRole(SnowflakeId roleId, string? groupName)
        {
            RoleId = roleId;
            GroupName = groupName;
        }
    }

    public class AccessibleGroupName
    {
        public string Name { get; }

        public AccessibleGroupName(string name)
        {
            Name = name;
        }
    }

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
}
