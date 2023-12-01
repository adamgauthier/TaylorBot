using Discord;

namespace TaylorBot.Net.Commands.DiscordNet.PageMessages;

public class DescriptionPageEditor(IReadOnlyList<string> pages) : IEmbedPageEditor
{
    public int PageCount => pages.Count;

    public EmbedBuilder Edit(EmbedBuilder embed, int currentPage)
    {
        return embed.WithDescription(pages[currentPage - 1]);
    }
}
