namespace TaylorBot.Net.Commands.PostExecution
{
    public interface ICommandUsageRepository
    {
        void QueueIncrementSuccessfulUseCount(string commandName);
        void QueueIncrementUnhandledErrorCount(string commandName);
        ValueTask PersistQueuedUsageCountIncrementsAsync();
    }
}
