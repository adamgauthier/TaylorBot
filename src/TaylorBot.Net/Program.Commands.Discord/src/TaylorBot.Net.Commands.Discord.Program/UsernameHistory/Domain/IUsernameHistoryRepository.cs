using Discord;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaylorBot.Net.Commands.Discord.Program.UsernameHistory.Domain
{
    public interface IUsernameHistoryRepository
    {
        record UsernameChange(string Username, DateTimeOffset ChangedAt);

        ValueTask<IReadOnlyList<UsernameChange>> GetUsernameHistoryFor(IUser user, int count);
    }
}
