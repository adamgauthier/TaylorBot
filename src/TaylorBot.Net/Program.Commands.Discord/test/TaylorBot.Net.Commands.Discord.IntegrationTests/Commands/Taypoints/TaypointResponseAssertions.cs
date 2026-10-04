using System.Globalization;
using System.Text.RegularExpressions;
using FluentAssertions;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Discord;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Scenarios;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Commands.Taypoints;

internal static partial class TaypointResponseAssertions
{
    public static void ShouldShowBalance(this DiscordExchange response, ScenarioUser user, long taypoints, int? rank = null)
    {
        var description = response.ShouldHaveSingleEmbedDescription().Replace("*", "", StringComparison.Ordinal);
        var balance = BalancePattern().Matches(description).Should().ContainSingle().Which;
        balance.Groups["user"].Value.Should().Be(user.Id);
        long.Parse(balance.Groups["amount"].Value, NumberStyles.Integer | NumberStyles.AllowThousands, CultureInfo.InvariantCulture)
            .Should().Be(taypoints);

        var ranks = RankPattern().Matches(description)
            .Select(match => int.Parse(match.Groups["rank"].Value, CultureInfo.InvariantCulture));
        ranks.Should().Equal(rank.HasValue ? [rank.Value] : []);
        if (rank.HasValue)
        {
            description.Should().MatchRegex(@"</taypoints leaderboard:\d+>");
        }
    }

    [GeneratedRegex(@"<@!?(?<user>\d+)>\s+has\s+(?<amount>-?[\d,]+)\s+taypoints?\b", RegexOptions.CultureInvariant, 1000)]
    private static partial Regex BalancePattern();

    [GeneratedRegex(@"\b(?<rank>\d+)(?:st|nd|rd|th)\s+in this server's\b", RegexOptions.CultureInvariant, 1000)]
    private static partial Regex RankPattern();
}
