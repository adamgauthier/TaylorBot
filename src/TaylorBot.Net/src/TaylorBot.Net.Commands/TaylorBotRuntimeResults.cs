using Discord;
using Discord.Commands;

namespace TaylorBot.Net.Commands
{
    public class TaylorBotEmptyResult : RuntimeResult
    {
        public TaylorBotEmptyResult() : base(error: null, reason: null) { }
    }

    public class TaylorBotEmbedResult : RuntimeResult
    {
        public Embed Embed { get; }

        public TaylorBotEmbedResult(Embed embed) : base(error: null, reason: null)
        {
            Embed = embed;
        }
    }
}
