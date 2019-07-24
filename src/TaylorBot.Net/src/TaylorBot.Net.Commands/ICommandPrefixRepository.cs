using Discord;
using System.Threading.Tasks;

namespace TaylorBot.Net.Commands
{
    public interface ICommandPrefixRepository
    {
        Task<string> GetOrInsertGuildPrefixAsync(IGuild guild);
    }
}
