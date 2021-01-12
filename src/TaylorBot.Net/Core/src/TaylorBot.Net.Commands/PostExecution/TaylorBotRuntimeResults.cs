using Discord;
using Discord.Commands;
using TaylorBot.Net.Commands.PageMessages;

namespace TaylorBot.Net.Commands.PostExecution
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

    public class TaylorBotPageMessageResult : RuntimeResult
    {
        public PageMessage PageMessage { get; }

        public TaylorBotPageMessageResult(PageMessage pageMessage) : base(error: null, reason: null)
        {
            PageMessage = pageMessage;
        }
    }
}
