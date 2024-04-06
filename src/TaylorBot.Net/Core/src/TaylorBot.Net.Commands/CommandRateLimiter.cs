using Microsoft.Extensions.Options;
using System.Globalization;
using TaylorBot.Net.Commands.Options;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands;

public interface IRateLimitRepository
{
    ValueTask<long> IncrementUsageAsync(string key);
}

public interface IRateLimiter
{
    ValueTask<RateLimitedResult?> VerifyDailyLimitAsync(DiscordUser user, string action);
}

public class CommandRateLimiter(IOptionsMonitor<CommandApplicationOptions> options, IRateLimitRepository rateLimitRepository, IPlusRepository plusRepository) : IRateLimiter
{
    public async ValueTask<RateLimitedResult?> VerifyDailyLimitAsync(DiscordUser user, string action)
    {
        var date = DateTimeOffset.UtcNow.ToString("yyyy'-'MM'-'dd", CultureInfo.InvariantCulture);
        var key = $"user:{user.Id}:action:{action}:date:{date}";

        var dailyUseCount = await rateLimitRepository.IncrementUsageAsync(key);

        var limit = options.CurrentValue.DailyLimits[action];

        var (userLimit, friendlyName) = limit.MaxUsesForPlusUser.HasValue && await plusRepository.IsActivePlusUserAsync(user) ?
            (limit.MaxUsesForPlusUser.Value, $"{limit.FriendlyName} (**TaylorBot Plus**)") :
            (limit.MaxUsesForUser, limit.FriendlyName);

        if (dailyUseCount <= userLimit)
        {
            return null;
        }
        else
        {
            return new RateLimitedResult(friendlyName, dailyUseCount, userLimit);
        }
    }
}
