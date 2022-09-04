using System.Threading.Tasks;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Weather.Domain
{
    public interface ILocationResult { }
    public record LocationFoundResult(Location Location) : ILocationResult;
    public record LocationNotFoundResult() : ILocationResult;
    public record LocationGenericErrorResult() : ILocationResult;

    public interface ILocationClient
    {
        ValueTask<ILocationResult> GetLocationAsync(string search);
    }
}
