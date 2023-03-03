using Discord;
using Humanizer;
using TaylorBot.Net.Commands.Discord.Program.Modules.UrbanDictionary.Domain;
using TaylorBot.Net.Commands.DiscordNet.PageMessages;
using TaylorBot.Net.Commands.PageMessages;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.UrbanDictionary.Commands;

public class UrbanDictionaryEditor : IMessageEditor, IEmbedPageEditor
{
    private readonly UrbanDictionaryResult _searchResult;

    public int PageCount => _searchResult.Definitions.Count;

    public UrbanDictionaryEditor(UrbanDictionaryResult searchResult)
    {
        _searchResult = searchResult;
    }

    public MessageContent Edit(int currentPage)
    {
        var page = _searchResult.Definitions[currentPage - 1];
        var embed = new EmbedBuilder();

        EditEmbed(embed, page);

        return new(embed.Build());
    }

    public EmbedBuilder Edit(EmbedBuilder embed, int currentPage)
    {
        var page = _searchResult.Definitions[currentPage - 1];

        EditEmbed(embed, page);

        embed.Description += "\n\nUse </urbandictionary:1080373890020282508> instead! 😊";

        return embed;
    }

    private static void EditEmbed(EmbedBuilder embed, UrbanDictionaryResult.SlangDefinition page)
    {
        embed
            .WithColor(TaylorBotColors.SuccessColor)
            .WithTitle(page.Word)
            .WithSafeUrl(page.Link.Replace("http:", "https:"))
            .WithDescription(page.Definition.Truncate(EmbedBuilder.MaxDescriptionLength))
            .AddField("Votes", $"👍 `{page.UpvoteCount}` | `{page.DownvoteCount}` 👎", inline: true)
            .WithThumbnailUrl("https://i.imgur.com/egsQhin.png")
            .WithFooter(page.Author)
            .WithTimestamp(page.WrittenOn);
    }
}
