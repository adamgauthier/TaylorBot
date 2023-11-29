using Discord;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Jail.Domain
{
    public record JailRole(SnowflakeId RoleId);

    public interface IJailRepository
    {
        ValueTask SetJailRoleAsync(IGuild guild, IRole jailRole);
        ValueTask<JailRole?> GetJailRoleAsync(IGuild guild);
    }
}
