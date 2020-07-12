using Discord.Commands;

namespace TaylorBot.Net.Commands
{
    public class TaylorBotPreconditionResult : PreconditionResult
    {
        public string? UserReason { get; }

        protected TaylorBotPreconditionResult(CommandError? error, string errorReason, string? userReason) : base(error, errorReason)
        {
            UserReason = userReason;
        }

        public static TaylorBotPreconditionResult FromPrivateError(string privateReason)
        {
            return new TaylorBotPreconditionResult(CommandError.UnmetPrecondition, privateReason, null);
        }

        public static TaylorBotPreconditionResult FromUserError(string privateReason, string userReason)
        {
            return new TaylorBotPreconditionResult(CommandError.UnmetPrecondition, privateReason, userReason);
        }
    }
}
