﻿using Discord;
using Humanizer;
using TaylorBot.Net.Commands.Discord.Program.Modules.UrbanDictionary.Domain;
using TaylorBot.Net.Commands.DiscordNet.PageMessages;
using TaylorBot.Net.Commands.PageMessages;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.UrbanDictionary.Commands;

public class UrbanDictionaryEditor(UrbanDictionaryResult searchResult) : IMessageEditor, IEmbedPageEditor
{
    public int PageCount => searchResult.Definitions.Count;

    public MessageContent Edit(int currentPage)
    {
        EmbedBuilder embed = new();
        EditEmbed(embed, currentPage);
        return new(embed.Build());
    }

    public EmbedBuilder Edit(EmbedBuilder embed, int currentPage)
    {
        EditEmbed(embed, currentPage, isLegacy: true);
        return embed;
    }

    private void EditEmbed(EmbedBuilder embed, int currentPage, bool isLegacy = false)
    {
        embed.WithThumbnailUrl("https://i.imgur.com/egsQhin.png");

        if (PageCount > 0)
        {
            var page = searchResult.Definitions[currentPage - 1];

            var description = page.Definition.Truncate(EmbedBuilder.MaxDescriptionLength);

            embed
                .WithColor(TaylorBotColors.SuccessColor)
                .WithTitle(page.Word)
                .WithSafeUrl(page.Link.Replace("http:", "https:", StringComparison.InvariantCulture))
                .WithDescription(isLegacy
                    ?
                    $"""
                    {description}

                    Use </urbandictionary:1080373890020282508> instead! 😊
                    """
                    : description)
                .AddField("Votes", $"👍 `{page.UpvoteCount}` | `{page.DownvoteCount}` 👎", inline: true)
                .WithFooter(page.Author)
                .WithTimestamp(page.WrittenOn);
        }
        else
        {
            embed
                .WithColor(TaylorBotColors.ErrorColor)
                .WithDescription(
                    """
                    No definition found on UrbanDictionary 😕
                    Did you misspell your slang term? 🤔
                    """);
        }
    }
}
