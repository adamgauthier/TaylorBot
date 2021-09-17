using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaylorBot.Net.Commands
{
    public interface ICommandRepository
    {
        public record Command(string Name, string ModuleName);

        ValueTask<IReadOnlyCollection<Command>> GetAllCommandsAsync();
        ValueTask<Command?> FindCommandByAliasAsync(string alias);
    }
}
