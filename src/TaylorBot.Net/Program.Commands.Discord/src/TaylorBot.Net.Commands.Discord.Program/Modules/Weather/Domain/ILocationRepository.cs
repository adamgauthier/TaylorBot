using Discord;
using System.Threading.Tasks;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Weather.Domain
{
    public record Location(string Latitude, string Longitude, string FormattedAddress);

    public interface ILocationRepository
    {
        ValueTask<Location?> GetLocationAsync(IUser user);
    }
}
