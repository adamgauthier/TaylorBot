using FluentAssertions;
using System.Text.Json;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Discord;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Scenarios;

internal static class ConfigurationAuditAssertions
{
    public static void ShouldHaveConfigurationChanges(this DiscordApi api, ScenarioGuild guild, ScenarioUser moderator,
        params (string Setting, string Before, string After)[] changes)
    {
        var messages = api.ModerationLogs(guild);
        messages.Should().HaveCount(changes.Length);
        for (var index = 0; index < changes.Length; index++)
        {
            var message = messages[index].Body!.Value;
            var embed = message.GetProperty("embeds").EnumerateArray().Should().ContainSingle().Which;
            embed.GetProperty("description").GetString()!.Replace("**", "", StringComparison.Ordinal).Should().EndWith(changes[index].Setting);
            Field(embed, "Moderator").Should().Contain(moderator.Id);
            Field(embed, "Before").Should().Be(changes[index].Before);
            Field(embed, "After").Should().Be(changes[index].After);
            embed.GetProperty("timestamp").GetDateTimeOffset().Should().BeOnOrBefore(DateTimeOffset.UtcNow);
            var allowedMentions = message.GetProperty("allowed_mentions");
            allowedMentions.ValueKind.Should().Be(JsonValueKind.Object);
            foreach (var name in new[] { "parse", "roles", "users" })
            {
                var mentions = allowedMentions.GetProperty(name);
                mentions.ValueKind.Should().BeOneOf(JsonValueKind.Null, JsonValueKind.Array);
                if (mentions.ValueKind == JsonValueKind.Array)
                {
                    mentions.EnumerateArray().Should().BeEmpty();
                }
            }
        }
    }

    private static string Field(JsonElement embed, string name) => embed.GetProperty("fields").EnumerateArray()
        .Single(field => field.GetProperty("name").GetString() == name).GetProperty("value").GetString()!;
}
