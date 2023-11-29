using Discord;

namespace TaylorBot.Net.Commands.PageMessages;

public class EmbedDescriptionTextEditor : IMessageEditor
{
    private readonly EmbedBuilder _baseEmbed;
    private readonly IReadOnlyList<string> _pages;
    private readonly bool _hasPageFooter;
    private readonly string _emptyText;

    public int PageCount => _pages.Count;

    public EmbedDescriptionTextEditor(EmbedBuilder baseEmbed, IReadOnlyList<string> pages, bool hasPageFooter, string emptyText)
    {
        _baseEmbed = baseEmbed;
        _pages = pages;
        _hasPageFooter = hasPageFooter;
        _emptyText = emptyText;
    }

    public MessageContent Edit(int currentPage)
    {
        if (_pages.Count > 0)
        {
            _baseEmbed.WithDescription(_pages[currentPage - 1]);

            if (_hasPageFooter)
            {
                _baseEmbed.WithFooter($"Page {currentPage}/{PageCount}");
            }
        }
        else
        {
            _baseEmbed.WithDescription(_emptyText);
        }

        return new(_baseEmbed.Build());
    }
}
