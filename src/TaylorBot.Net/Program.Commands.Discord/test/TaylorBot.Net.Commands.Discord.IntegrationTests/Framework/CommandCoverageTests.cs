using System.Reflection;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Framework;

public sealed class CommandCoverageTests
{
    [Fact]
    public void EveryUnblockedDeployedCommandHasDeclaredBehavioralScenarios()
    {
        var deployed = Directory.EnumerateFiles(Path.Combine(AppContext.BaseDirectory, "Fixtures", "Commands"), "*.json", SearchOption.AllDirectories)
            .SelectMany(ReadRoutes).ToHashSet(StringComparer.Ordinal);
        // The manually provisioned valentines2026 schema is deliberately outside this suite's bootstrap.
        deployed.ExceptWith(["love spread", "love history", "love ready", "love leaderboard"]);
        var covered = typeof(CommandCoverageTests).Assembly.GetTypes()
            .Where(type => type.GetMethods().Any(IsActiveTest))
            .SelectMany(type => type.GetCustomAttributes<TraitAttribute>()
                .Concat(type.GetMethods().Where(IsActiveTest).SelectMany(method => method.GetCustomAttributes<TraitAttribute>())))
            .Where(trait => trait.Name == "Command")
            .Select(trait => trait.Value).ToHashSet(StringComparer.Ordinal);

        covered.Should().BeEquivalentTo(deployed,
            "every unblocked deployed route needs meaningful integration scenarios, and coverage declarations must not refer to retired or misspelled routes");
    }

    private static bool IsActiveTest(MethodInfo method) =>
        method.GetCustomAttribute<FactAttribute>() is { Skip: null };

    private static IEnumerable<string> ReadRoutes(string path)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return [.. ReadRoutes(document.RootElement)];
    }

    private static IEnumerable<string> ReadRoutes(JsonElement command, string? prefix = null)
    {
        var name = prefix == null ? command.GetProperty("name").GetString()! : $"{prefix} {command.GetProperty("name").GetString()}";
        var children = command.TryGetProperty("options", out var options)
            ? options.EnumerateArray().Where(option => option.GetProperty("type").GetInt32() is 1 or 2).ToArray()
            : [];
        return children.Length == 0 ? [name] : children.SelectMany(child => ReadRoutes(child, name));
    }
}
