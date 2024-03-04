using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Domain;
using TaylorBot.Net.Core.Tasks;

namespace TaylorBot.Net.Commands.Discord.Program.Services;

public class TaypointGuildCacheUpdater(ITaypointBalanceRepository taypointBalanceRepository, TaskExceptionLogger taskExceptionLogger)
{
    public void UpdateLastKnownPointCountInBackground(IUser user, long updatedCount)
    {
        if (!user.IsBot && user is IGuildUser guildUser)
        {
            _ = Task.Run(async () => await taskExceptionLogger.LogOnError(
                async () => await taypointBalanceRepository.UpdateLastKnownPointCountAsync(guildUser, updatedCount),
                nameof(taypointBalanceRepository.UpdateLastKnownPointCountAsync)
            ));
        }
    }
}
