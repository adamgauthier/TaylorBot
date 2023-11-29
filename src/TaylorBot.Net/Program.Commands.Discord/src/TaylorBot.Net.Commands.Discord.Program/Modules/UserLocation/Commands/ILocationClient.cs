namespace TaylorBot.Net.Commands.Discord.Program.Modules.UserLocation.Commands;

public interface ILocationResult { }
public record LocationFoundResult(Location Location) : ILocationResult;
public record LocationNotFoundResult() : ILocationResult;
public record LocationGenericErrorResult() : ILocationResult;

public interface ITimeZoneResult { }
public record TimeZoneResult(string TimeZoneId) : ITimeZoneResult;
public record TimeZoneGenericErrorResult() : ITimeZoneResult;

public interface ILocationClient
{
    ValueTask<ILocationResult> GetLocationAsync(string search);
    ValueTask<ITimeZoneResult> GetTimeZoneForLocationAsync(string latitude, string longitude);
}
