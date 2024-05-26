using Discord;
using FakeItEasy;
using TaylorBot.Net.Commands.PostExecution;
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

    public static RunContext CreateTestContext(ISlashCommand command)
    {
        CommandGuild guild = new(167845806479638529, A.Fake<IGuild>());
        CommandChannel channel = new(167845806479638529, ChannelType.Text);

        return new RunContext(DateTimeOffset.UtcNow, AUser, null, channel, guild, null!, null!, new("922354806574678086", command.Info.Name), null!, null!, null!);
    }
}
