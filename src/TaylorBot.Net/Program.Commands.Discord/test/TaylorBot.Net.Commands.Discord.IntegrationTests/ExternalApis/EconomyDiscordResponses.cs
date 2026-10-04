using System.Net;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Discord;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Scenarios;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.ExternalApis;

public static class EconomyDiscordResponses
{
    public static void ExpectHeistResult(this DiscordApi api, ScenarioGuild guild) =>
        api.ExpectRequest("POST", $"channels/{guild.ChannelId}/messages",
            new { id = "100000000000099999", channel_id = guild.ChannelId, content = "", timestamp = "2026-01-01T00:00:00Z" }, HttpStatusCode.OK);

    public static string HeistResult(this DiscordApi api, ScenarioGuild guild)
    {
        var request = api.RequestsFor("POST", $"channels/{guild.ChannelId}/messages").Single();
        return request.Body!.Value.GetProperty("embeds")[0].GetProperty("description").GetString()!;
    }
}
