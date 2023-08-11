using Discord;
using OperationResult;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Embed;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.UserLocation.Commands;

public class LocationFetcherDomainService
{
    private readonly IRateLimiter _rateLimiter;
    private readonly ILocationClient _locationClient;

    public LocationFetcherDomainService(IRateLimiter rateLimiter, ILocationClient locationClient)
    {
        _rateLimiter = rateLimiter;
        _locationClient = locationClient;
    }

    public async Task<Result<Location, ICommandResult>> GetLocationAsync(IUser author, string location)
    {
        var placeRateLimit = await _rateLimiter.VerifyDailyLimitAsync(author, "google-places-search");
        if (placeRateLimit != null)
            return Error((ICommandResult)placeRateLimit);

        var locationResult = await _locationClient.GetLocationAsync(location);
        switch (locationResult)
        {
            case LocationGenericErrorResult _:
                return Error((ICommandResult)new EmbedResult(EmbedFactory.CreateError(
                    """
                    Unexpected error happened when attempting to find this location. 😢
                    The location service might be down. Try again later!
                    """)));

            case LocationNotFoundResult _:
                return Error((ICommandResult)new EmbedResult(EmbedFactory.CreateError(
                    """
                    Unable to find the location you specified. 🔍
                    Are you sure it's a real place that exist in the world?
                    """)));

            case LocationFoundResult found:
                return Ok(found.Location);

            default: throw new NotImplementedException();
        }
    }
}

