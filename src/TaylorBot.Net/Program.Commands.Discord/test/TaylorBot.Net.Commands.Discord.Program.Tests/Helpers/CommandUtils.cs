using FakeItEasy;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Tests.Helpers;

public class CommandUtils
{
    public static readonly DiscordUser AUser = new(119341483219353602, "adam", "ecb368bd7eb540754c0bf5a2ce65af62", "0", IsBot: true, MemberInfo: null);

    public static IRateLimiter UnlimitedRateLimiter
    {
        get
        {
            var rateLimiter = A.Fake<IRateLimiter>(o => o.Strict());
            A.CallTo(() => rateLimiter.VerifyDailyLimitAsync(A<DiscordUser>.Ignored, A<string>.Ignored)).Returns(null);
            return rateLimiter;
        }
    }
}
