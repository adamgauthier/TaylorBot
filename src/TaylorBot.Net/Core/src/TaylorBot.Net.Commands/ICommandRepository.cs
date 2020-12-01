using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaylorBot.Net.Commands
{
    public interface ICommandRepository
    {
        public class Command
        {
            public string Name { get; }
            public string ModuleName { get; }

            public Command(string name, string moduleName)
            {
                Name = name;
                ModuleName = moduleName;
            }
        }

        ValueTask<IReadOnlyCollection<Command>> GetAllCommandsAsync();
        ValueTask<Command?> FindCommandByAliasAsync(string alias);
    }
}
