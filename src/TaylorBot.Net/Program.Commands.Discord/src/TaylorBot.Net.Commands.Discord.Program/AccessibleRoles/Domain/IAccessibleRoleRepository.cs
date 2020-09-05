using Discord;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Commands.Discord.Program.AccessibleRoles.Domain
{
    public class AccessibleRole
    {
        public SnowflakeId RoleId { get; }

        public AccessibleRole(SnowflakeId roleId)
        {
            RoleId = roleId;
        }
    }

    public interface IAccessibleRoleRepository
    {
        ValueTask<bool> IsRoleAccessibleAsync(IRole role);
        ValueTask<IReadOnlyCollection<AccessibleRole>> GetAccessibleRolesAsync(IGuild guild);
    }
}
