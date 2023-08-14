using Discord;
using System;
using System.Collections.Generic;
using TaylorBot.Net.Commands.Discord.Program.Modules.Image.Domain;
using TaylorBot.Net.Commands.DiscordNet.PageMessages;
using TaylorBot.Net.Core.Colors;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Image.Commands;

public class CustomSearchImagePageEditor : IEmbedPageEditor
{
    private readonly IReadOnlyList<ImageResult> _pages;

    public int PageCount => _pages.Count;

    public CustomSearchImagePageEditor(IReadOnlyList<ImageResult> pages)
    {
        _pages = pages;
    }

    public EmbedBuilder Edit(EmbedBuilder embed, int currentPage)
    {
        if (PageCount > 0)
        {
            var page = _pages[currentPage - 1];

            if (Uri.IsWellFormedUriString(page.PageUrl, UriKind.Absolute))
                embed.WithUrl(page.PageUrl);

            return embed
                .WithTitle(page.Title)
                .WithImageUrl(page.ImageUrl);
        }
        else
        {
            return embed
                .WithColor(TaylorBotColors.ErrorColor)
                .WithDescription(
                    """
                    No images found for your search 😕
                    Did you misspell a word? 🤔
                    """);
        }
    }
}
