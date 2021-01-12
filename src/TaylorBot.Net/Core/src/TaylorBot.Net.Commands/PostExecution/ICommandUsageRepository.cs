using Discord.Commands;

namespace TaylorBot.Net.Commands.PostExecution
{
    public interface ICommandUsageRepository
    {
        void AddSuccessfulUseCount(CommandInfo command);
        void AddUnhandledErrorCount(CommandInfo command);
    }
}
