using Discord;
using Discord.Commands;

namespace TaylorBot.Net.Commands.DiscordNet
{
    public class TaylorBotResult : RuntimeResult
    {
        public ICommandResult Result { get; }
        public RunContext Context { get; }

        public TaylorBotResult(ICommandResult result, RunContext context) : base(error: null, reason: null)
        {
            Result = result;
            Context = context;
        }
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
