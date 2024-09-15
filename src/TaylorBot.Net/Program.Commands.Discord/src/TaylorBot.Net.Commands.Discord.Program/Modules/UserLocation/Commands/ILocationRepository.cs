using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.UserLocation.Commands;

public record Location(string Latitude, string Longitude, string FormattedAddress, bool? IsGeneral);

public record StoredLocation(Location Location, string TimeZoneId);

public interface ILocationRepository
{
    ValueTask<StoredLocation?> GetLocationAsync(DiscordUser user);
    ValueTask SetLocationAsync(DiscordUser user, StoredLocation location);
    ValueTask ClearLocationAsync(DiscordUser user);
}
