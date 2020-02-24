using Discord;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Preconditions;

namespace TaylorBot.Net.Commands.Infrastructure
{
    public class OnGoingCommandInMemoryRepository : IOngoingCommandRepository
    {
        private readonly IDictionary<ulong, long> ongoingCommands = new ConcurrentDictionary<ulong, long>();

        public Task AddOngoingCommandAsync(IUser user)
        {
            if (ongoingCommands.TryGetValue(user.Id, out var count))
            {
                ongoingCommands.Remove(user.Id);
                ongoingCommands.Add(user.Id, count + 1);
            }
            else
            {
                ongoingCommands.Add(user.Id, 1);
            }

            return Task.CompletedTask;
        }

        public Task<bool> HasAnyOngoingCommandAsync(IUser user)
        {
            return Task.FromResult(
                ongoingCommands.TryGetValue(user.Id, out var count) && count > 0
            );
        }

        public Task RemoveOngoingCommandAsync(IUser user)
        {
            if (ongoingCommands.TryGetValue(user.Id, out var count))
            {
                ongoingCommands.Remove(user.Id);
                ongoingCommands.Add(user.Id, count - 1);
            }

            return Task.CompletedTask;
        }
    }
}
