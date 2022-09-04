using Discord;
using FakeItEasy;

namespace TaylorBot.Net.Commands.Discord.Program.Tests.Helpers
{
    public class CommandUtils
    {
        public static IRateLimiter UnlimitedRateLimiter
        {
            get
            {
                var rateLimiter = A.Fake<IRateLimiter>(o => o.Strict());
                A.CallTo(() => rateLimiter.VerifyDailyLimitAsync(A<IUser>.Ignored, A<string>.Ignored)).Returns(null);
                return rateLimiter;
            }
        }
    }
}
