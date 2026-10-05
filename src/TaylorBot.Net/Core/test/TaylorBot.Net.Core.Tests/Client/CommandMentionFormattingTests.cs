using FakeItEasy;
using FluentAssertions;
using System.Globalization;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Snowflake;
using Xunit;

namespace TaylorBot.Net.Core.Tests.Client;

public class CommandMentionFormattingTests
{
    private readonly IApplicationCommandsRepository repository = A.Fake<IApplicationCommandsRepository>(o => o.Strict());
    private readonly SlashCommandMentioner mention;

    public CommandMentionFormattingTests()
    {
        mention = new(repository);
    }

    [Fact]
    public async Task Format_LeavesOrdinaryTextAndArgumentsLiteral()
    {
        const bool waitForRefresh = true;
        A.CallTo(() => repository.GetCommandIdAsync("help", waitForRefresh)).Returns(new ValueTask<SnowflakeId?>(new SnowflakeId("42")));
        const string userText = "`/unknown` {0} {{braces}}";

        var result = await mention.FormatAsync($"Literal `/birthday set`: {userText}. Use {mention.Slash("help")}.");

        result.Should().Be("Literal `/birthday set`: `/unknown` {0} {{braces}}. Use </help:42>.");
        A.CallTo(() => repository.GetCommandIdAsync("help", waitForRefresh)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Format_ResolvesRepeatedReferencesAcrossNestedFragmentsOnce()
    {
        const bool waitForRefresh = true;
        A.CallTo(() => repository.GetCommandIdAsync("help", waitForRefresh)).Returns(new ValueTask<SnowflakeId?>(new SnowflakeId("42")));
        FormattableString fragment = $"Start with {mention.Slash("help")}.";

        var result = await mention.FormatAsync($"{fragment} See {mention.Slash("help")} again.");

        result.Should().Be("Start with </help:42>. See </help:42> again.");
        A.CallTo(() => repository.GetCommandIdAsync("help", waitForRefresh)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Format_StartsIndependentLookupsTogether()
    {
        const bool waitForRefresh = true;
        TaskCompletionSource<SnowflakeId?> help = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource<SnowflakeId?> birthday = new(TaskCreationOptions.RunContinuationsAsynchronously);
        A.CallTo(() => repository.GetCommandIdAsync("help", waitForRefresh)).Returns(new ValueTask<SnowflakeId?>(help.Task));
        A.CallTo(() => repository.GetCommandIdAsync("birthday", waitForRefresh)).Returns(new ValueTask<SnowflakeId?>(birthday.Task));

        var formatting = mention.FormatAsync($"{mention.Slash("help")} and {mention.Slash("birthday set")}");

        A.CallTo(() => repository.GetCommandIdAsync("help", waitForRefresh)).MustHaveHappenedOnceExactly();
        A.CallTo(() => repository.GetCommandIdAsync("birthday", waitForRefresh)).MustHaveHappenedOnceExactly();
        help.SetResult(new("42"));
        birthday.SetResult(new("43"));
        (await formatting).Should().Be("</help:42> and </birthday set:43>");
    }

    [Fact]
    public async Task Format_PreservesCultureFormatSpecifiersAlignmentAndBraces()
    {
        const bool waitForRefresh = true;
        A.CallTo(() => repository.GetCommandIdAsync("help", waitForRefresh)).Returns(new ValueTask<SnowflakeId?>(new SnowflakeId("42")));
        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            const decimal amount = 12.5m;

            var result = await mention.FormatAsync($"{amount:F1}|{mention.Slash("help"),15}|{{literal}}");

            result.Should().Be("12,5|     </help:42>|{literal}");
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public async Task Format_UsesMatchingInteractionIdAndPropagatesNoWaitPolicy()
    {
        const bool waitForRefresh = false;
        A.CallTo(() => repository.GetCommandIdAsync("birthday", waitForRefresh)).Returns(new ValueTask<SnowflakeId?>((SnowflakeId?)null));

        var result = await mention.FormatAsync($"{mention.Slash("help")} {mention.Slash("birthday set")}",
            waitForRefresh: false, interactionCommand: ("help", new("42")));

        result.Should().Be("</help:42> **/birthday set**");
        A.CallTo(() => repository.GetCommandIdAsync("help", A<bool>.Ignored)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Format_DistinguishesGlobalGuildAndPrefixReferences()
    {
        const bool waitForRefresh = true;
        SnowflakeId guild = new("99");
        A.CallTo(() => repository.GetCommandIdAsync("help", waitForRefresh)).Returns(new ValueTask<SnowflakeId?>(new SnowflakeId("42")));
        A.CallTo(() => repository.GetGuildCommandIdAsync(guild, "help", waitForRefresh)).Returns(new ValueTask<SnowflakeId?>(new SnowflakeId("43")));

        var result = await mention.FormatAsync($"{mention.Slash("help")} {mention.GuildSlash("help")} {mention.Prefix("help")}", guildId: guild);

        result.Should().Be("</help:42> </help:43> **help**");
    }

    [Fact]
    public async Task Format_JoinsCapturedFragmentsWithoutParsingTheSeparator()
    {
        const bool waitForRefresh = true;
        A.CallTo(() => repository.GetCommandIdAsync("help", waitForRefresh)).Returns(new ValueTask<SnowflakeId?>(new SnowflakeId("42")));
        FormattableString[] fragments = [$"See {mention.Slash("help")}", $"Use {mention.Slash("help")}"];

        var result = await mention.FormatAsync($"Commands: {mention.Join("\n{0}\n", fragments)}.");

        result.Should().Be("Commands: See </help:42>\n{0}\nUse </help:42>.");
        A.CallTo(() => repository.GetCommandIdAsync("help", waitForRefresh)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Format_JoinsEmptyFragmentsWithoutLookingUpCommands()
    {
        var result = await mention.FormatAsync($"Before{mention.Join("\n", [])}After");

        result.Should().Be("BeforeAfter");
        Fake.GetCalls(repository).Should().BeEmpty();
    }

    [Fact]
    public async Task Format_RejectsGuildReferencesWithoutGuildContext()
    {
        var action = () => mention.FormatAsync($"{mention.GuildSlash("signature")}");

        await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("*guild context*");
    }

    [Fact]
    public void Reference_RejectsOrdinaryStringInterpolation()
    {
        var reference = mention.Slash("help");

        Action action = () => _ = $"Use {reference}";

        action.Should().Throw<FormatException>().WithMessage("*FormatAsync*");
        Fake.GetCalls(repository).Should().BeEmpty();
    }
}
