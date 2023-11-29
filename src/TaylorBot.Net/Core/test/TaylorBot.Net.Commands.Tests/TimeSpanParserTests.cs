using FluentAssertions;
using FluentAssertions.Extensions;
using System.Text.Json;
using TaylorBot.Net.Commands.Parsers;
using Xunit;

namespace TaylorBot.Net.Commands.Types.Tests
{
    public class TimeSpanParserTests
    {
        private readonly TimeSpanParser _timeSpanParser = new();

        [Fact]
        public async Task ParseAsync_WhenSingleHourComponent_ThenParsedCorrectly()
        {
            JsonElement element = CreateJsonElement("5h");

            var result = await _timeSpanParser.ParseAsync(null!, element, null!);

            result.Value.Value.Should().Be(5.Hours());
        }

        [Fact]
        public async Task ParseAsync_WhenMultipleComponents_ThenParsedCorrectly()
        {
            JsonElement element = CreateJsonElement("1d 11h");

            var result = await _timeSpanParser.ParseAsync(null!, element, null!);

            result.Value.Value.Should().Be(1.Days().And(11.Hours()));
        }

        [Fact]
        public async Task ParseAsync_WhenDuplicateComponents_ThenError()
        {
            JsonElement element = CreateJsonElement("5m 10m");

            var result = await _timeSpanParser.ParseAsync(null!, element, null!);

            result.Error.Should().NotBeNull();
        }

        private static JsonElement CreateJsonElement(string value)
        {
            var json = "{ \"Value\": \"" + value + "\" }";
            var element = JsonSerializer.Deserialize<JsonElement>(json).GetProperty("Value");
            return element;
        }
    }
}
