using Discord;

namespace TaylorBot.Net.Commands.PageMessages;

public class EmbedDescriptionTextEditor(EmbedBuilder baseEmbed, IReadOnlyList<string> pages, bool hasPageFooter, string emptyText) : IMessageEditor
{
    public int PageCount => pages.Count;

    public MessageContent Edit(int currentPage)
    {
        if (pages.Count > 0)
        {
            baseEmbed.WithDescription(pages[currentPage - 1]);

            if (hasPageFooter)
            {
                baseEmbed.WithFooter($"Page {currentPage}/{PageCount}");
            }
        }
        else
        {
            baseEmbed.WithDescription(emptyText);
        }

        return new(baseEmbed.Build());
    }
}
