using Discord;
using System.Threading.Tasks;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.UserLocation.Commands;

public record Location(string Latitude, string Longitude, string FormattedAddress);

public record StoredLocation(Location Location, string TimeZoneId);

public interface ILocationRepository
{
    ValueTask<StoredLocation?> GetLocationAsync(IUser user);
    ValueTask SetLocationAsync(IUser user, StoredLocation location);
    ValueTask ClearLocationAsync(IUser user);
}
