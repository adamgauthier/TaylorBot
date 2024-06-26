﻿using System.Collections.Concurrent;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Infrastructure;

public class OnGoingCommandInMemoryRepository : IOngoingCommandRepository
{
    private readonly IDictionary<string, long> ongoingCommands = new ConcurrentDictionary<string, long>();

    private static string GetKey(DiscordUser user, string pool) => $"{user.Id}{pool}";

    public ValueTask AddOngoingCommandAsync(DiscordUser user, string pool)
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

        return default;
    }

    public ValueTask<bool> HasAnyOngoingCommandAsync(DiscordUser user, string pool)
    {
        return new ValueTask<bool>(
            ongoingCommands.TryGetValue(GetKey(user, pool), out var count) && count > 0
        );
    }

    public ValueTask RemoveOngoingCommandAsync(DiscordUser user, string pool)
    {
        var key = GetKey(user, pool);
        if (ongoingCommands.TryGetValue(key, out var count))
        {
            ongoingCommands.Remove(key);
            ongoingCommands.Add(key, count - 1);
        }

        return default;
    }
}
