using Discord;
using Microsoft.Extensions.Options;
using System;
using System.Globalization;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Options;

namespace TaylorBot.Net.Commands
{
    public interface IRateLimitRepository
    {
        ValueTask<long> IncrementUsageAsync(string key);
    }

    public interface IRateLimiter
    {
        ValueTask<RateLimitedResult?> VerifyDailyLimitAsync(IUser user, string action, string friendlyName);
    }

    public class RateLimiter : IRateLimiter
    {
        private readonly IOptionsMonitor<CommandApplicationOptions> _options;
        private readonly IRateLimitRepository _rateLimitRepository;

        public RateLimiter(IOptionsMonitor<CommandApplicationOptions> options, IRateLimitRepository rateLimitRepository)
        {
            _options = options;
            _rateLimitRepository = rateLimitRepository;
        }

        public async ValueTask<RateLimitedResult?> VerifyDailyLimitAsync(IUser user, string action, string friendlyName)
        {
            var date = DateTimeOffset.UtcNow.ToString("yyyy'-'MM'-'dd", CultureInfo.InvariantCulture);
            var key = $"user:{user.Id}:action:{action}:date:{date}";

            var dailyUseCount = await _rateLimitRepository.IncrementUsageAsync(key);

            var limit = _options.CurrentValue.DailyLimits[action];

            if (dailyUseCount <= limit)
            {
                return null;
            }
            else
            {
                return new RateLimitedResult(friendlyName, dailyUseCount, limit);
            }
        }
    }
}
