using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Image.Domain;
using TaylorBot.Net.Commands.PageMessages;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;

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
                .WithTitle(page.Title)
                .WithSafeUrl(page.PageUrl)
                .WithImageUrl(page.ImageUrl)
                .WithFooter($"{_search.ResultCount} results found in {_search.SearchTimeSeconds} seconds");

            return new(embed.Build());
        }
    }
}
