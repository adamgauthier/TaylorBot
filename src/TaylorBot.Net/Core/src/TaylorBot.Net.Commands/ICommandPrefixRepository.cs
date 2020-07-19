using Discord;
using System.Threading.Tasks;

namespace TaylorBot.Net.Commands
{
    public interface ICommandPrefixRepository
    {
        ValueTask<string> GetOrInsertGuildPrefixAsync(IGuild guild);
        ValueTask ChangeGuildPrefixAsync(IGuild guild, string prefix);
    }
}
