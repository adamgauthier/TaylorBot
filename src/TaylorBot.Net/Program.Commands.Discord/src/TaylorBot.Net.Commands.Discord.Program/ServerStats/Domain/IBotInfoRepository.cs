using System.Threading.Tasks;

namespace TaylorBot.Net.Commands.Discord.Program.ServerStats.Domain
{
    public interface IBotInfoRepository
    {
        ValueTask<string> GetProductVersionAsync();
    }
}
