using Discord.Commands;

namespace TaylorBot.Net.Commands
{
    public interface ICommandUsageRepository
    {
        void AddSuccessfulUseCount(CommandInfo command);
        void AddUnhandledErrorCount(CommandInfo command);
    }
}
