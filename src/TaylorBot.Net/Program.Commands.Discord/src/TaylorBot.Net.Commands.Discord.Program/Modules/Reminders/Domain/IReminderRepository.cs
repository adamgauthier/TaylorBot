using Discord;
using System;
using System.Threading.Tasks;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Reminders.Domain
{
    public interface IReminderRepository
    {
        ValueTask<long> GetReminderCountAsync(IUser user);
        ValueTask AddReminderAsync(IUser user, DateTimeOffset remindAt, string text);
    }
}
