using OperationResult;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.User;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.UserLocation.Commands;

public class LocationFetcherDomainService(IRateLimiter rateLimiter, ILocationClient locationClient)
{
    public async Task<Result<Location, ICommandResult>> GetLocationAsync(DiscordUser author, string location)
    {
        var placeRateLimit = await rateLimiter.VerifyDailyLimitAsync(author, "google-places-search");
        if (placeRateLimit != null)
            return Error((ICommandResult)placeRateLimit);

        var locationResult = await locationClient.GetLocationAsync(location);
        switch (locationResult)
        {
            case LocationGenericErrorResult _:
                return Error((ICommandResult)new EmbedResult(EmbedFactory.CreateError(
                    """
                    Unexpected error happened when attempting to find this location 😢
                    The location service might be down. Try again later!
                    """)));

            case LocationNotFoundResult _:
                return Error((ICommandResult)new EmbedResult(EmbedFactory.CreateError(
                    """
                    Unable to find the location you specified 🔍
                    Are you sure it's a place that exists in the real world?
                    """)));

            case LocationFoundResult found:
                return Ok(found.Location);

            default: throw new NotImplementedException();
        }
    }
}

