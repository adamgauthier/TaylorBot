using FluentAssertions;
using System.Text.Json;
using TaylorBot.Net.Core.Client;
using Xunit;

namespace TaylorBot.Net.Core.Tests.Client;

public class InteractionChannelTests
{
    [Theory]
    [InlineData("""{"id":"100000000000000001","type":0}""", JsonValueKind.Undefined)]
    [InlineData("""{"id":"100000000000000001","type":0,"topic":null,"parent_id":null}""", JsonValueKind.Null)]
    [InlineData("""{"id":"100000000000000001","type":0,"topic":"Topic","parent_id":"100000000000000002"}""", JsonValueKind.String)]
    public void PartialChannel_RoundTripPreservesMissingAndNullFields(string json, JsonValueKind expectedKind)
    {
        var channel = JsonSerializer.Deserialize<Interaction.PartialChannel>(json)!;

        var roundTrip = JsonSerializer.Deserialize<Interaction.PartialChannel>(JsonSerializer.Serialize(channel))!;

        roundTrip.topic.ValueKind.Should().Be(expectedKind);
        roundTrip.parent_id.ValueKind.Should().Be(expectedKind);
    }
}
