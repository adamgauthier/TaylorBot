using Discord;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Preconditions;

namespace TaylorBot.Net.Commands.Infrastructure
{
    public class OnGoingCommandInMemoryRepository : IOngoingCommandRepository
    {
        private readonly IDictionary<string, long> ongoingCommands = new ConcurrentDictionary<string, long>();

        private string GetKey(IUser user, string pool) => $"{user.Id}{pool}";

        public Task AddOngoingCommandAsync(IUser user, string pool)
        {
            var key = GetKey(user, pool);
            if (ongoingCommands.TryGetValue(key, out var count))
            {
                ongoingCommands.Remove(key);
                ongoingCommands.Add(key, count + 1);
            }
            else
            {
                ongoingCommands.Add(key, 1);
            }

            return Task.CompletedTask;
        }

        public Task<bool> HasAnyOngoingCommandAsync(IUser user, string pool)
        {
            return Task.FromResult(
                ongoingCommands.TryGetValue(GetKey(user, pool), out var count) && count > 0
            );
        }

        public Task RemoveOngoingCommandAsync(IUser user, string pool)
        {
            var key = GetKey(user, pool);
            if (ongoingCommands.TryGetValue(key, out var count))
            {
                ongoingCommands.Remove(key);
                ongoingCommands.Add(key, count - 1);
            }

            return Task.CompletedTask;
        }
    }
}
