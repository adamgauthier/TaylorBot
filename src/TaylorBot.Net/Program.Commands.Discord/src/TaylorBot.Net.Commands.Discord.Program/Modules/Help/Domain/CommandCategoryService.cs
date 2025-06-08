using Microsoft.Extensions.Caching.Memory;
using System.Reflection;
using System.Text.RegularExpressions;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Help.Domain;

public interface ICommandsHelpRepository
{
    Task<string?> GetCommandsHelpAsync();
}

public record CommandCategory(string Id, string Name, string Emoji, string Description);

public partial class CommandCategoryService(ICommandsHelpRepository commandsContentRepository, IMemoryCache memoryCache)
{
    private sealed record CurrentCategory(string Name, string Emoji);

    private async Task<Dictionary<string, CommandCategory>> GetCategoriesAsync()
    {
        var categories = await memoryCache.GetOrCreateAsync(
            "command-categories",
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24);

                var content = await commandsContentRepository.GetCommandsHelpAsync() ?? GetEmbeddedCommandsHelp();
                var lines = content.Split('\n');
                CurrentCategory? currentCategory = null;
                List<string> currentContent = [];
                List<CommandCategory> categories = [];

                void SaveCurrentCategory()
                {
                    if (currentCategory != null)
                    {
                        categories.Add(new CommandCategory(
                            Id: currentCategory.Name.ToUpperInvariant().Replace(" ", "-", StringComparison.OrdinalIgnoreCase),
                            Name: currentCategory.Name,
                            Emoji: currentCategory.Emoji,
                            Description: string.Join("\n", currentContent)
                        ));
                        currentContent = [];
                    }
                }

                foreach (var line in lines)
                {
                    var trimmedLine = line.Trim();

                    // Check for top-level header (#)
                    var headerMatch = HeaderRegex().Match(trimmedLine);
                    if (headerMatch.Success)
                    {
                        SaveCurrentCategory();

                        currentCategory = new(headerMatch.Groups[1].Value.Trim(), headerMatch.Groups[2].Value.Trim());
                    }
                    // If we're in a category, collect all content until the next top-level header
                    else if (currentCategory != null && !string.IsNullOrWhiteSpace(trimmedLine))
                    {
                        currentContent.Add(trimmedLine);
                    }
                }

                // Add the last category if there is one
                SaveCurrentCategory();

                return categories.ToDictionary(c => c.Id);
            });
        ArgumentNullException.ThrowIfNull(categories);
        return categories;
    }

    [GeneratedRegex(@"^# (.+?) ([^\s]+)$")]
    private static partial Regex HeaderRegex();

    private static string GetEmbeddedCommandsHelp()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceNames = assembly.GetManifestResourceNames();
        var commandsResourceName = resourceNames.Single(r => r.EndsWith("commands.md", StringComparison.OrdinalIgnoreCase));

        using var stream = assembly.GetManifestResourceStream(commandsResourceName);
        ArgumentNullException.ThrowIfNull(stream);

        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

    public async Task<CommandCategory> GetCategoryAsync(string categoryId)
    {
        var categories = await GetCategoriesAsync();
        return categories.GetValueOrDefault(categoryId) ?? throw new ArgumentException($"Category {categoryId} not found");
    }

    public async Task<IReadOnlyCollection<CommandCategory>> GetAllCategoriesAsync()
    {
        var categories = await GetCategoriesAsync();
        return categories.Values;
    }
}
