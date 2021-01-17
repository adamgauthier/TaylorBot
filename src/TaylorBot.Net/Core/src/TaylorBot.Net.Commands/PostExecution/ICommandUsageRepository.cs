using Discord.Commands;
using System.Threading.Tasks;

namespace TaylorBot.Net.Commands.PostExecution
{
    public interface ICommandUsageRepository
    {
        void QueueIncrementSuccessfulUseCount(CommandInfo command);
        void QueueIncrementUnhandledErrorCount(CommandInfo command);
        ValueTask PersistQueuedUsageCountIncrementsAsync();
    }
}
