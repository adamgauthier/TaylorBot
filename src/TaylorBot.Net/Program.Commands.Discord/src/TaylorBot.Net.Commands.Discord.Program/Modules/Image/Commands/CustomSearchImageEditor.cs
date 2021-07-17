using Discord;
using System;
using TaylorBot.Net.Commands.Discord.Program.Modules.Image.Domain;
using TaylorBot.Net.Commands.PageMessages;
using TaylorBot.Net.Core.Colors;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Image.Commands
{
    public class CustomSearchImageEditor : IMessageEditor
    {
        private readonly SuccessfulSearch _search;

        public int PageCount => _search.Images.Count;

        public CustomSearchImageEditor(SuccessfulSearch search)
        {
            _search = search;
        }

        public MessageContent Edit(int currentPage)
        {
            var page = _search.Images[currentPage - 1];

            var embed = new EmbedBuilder()
                .WithColor(TaylorBotColors.SuccessColor)
                .WithFooter($"{_search.ResultCount} results found in {_search.SearchTimeSeconds} seconds");

            if (Uri.IsWellFormedUriString(page.PageUrl, UriKind.Absolute))
                embed.WithUrl(page.PageUrl);

            return new(embed
                .WithTitle(page.Title)
                .WithImageUrl(page.ImageUrl)
            .Build());
        }
    }
}
