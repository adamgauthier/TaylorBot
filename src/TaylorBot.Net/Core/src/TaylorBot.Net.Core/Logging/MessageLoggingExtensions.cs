using Discord;

namespace TaylorBot.Net.Core.Logging
{
    public static class MessageLoggingExtensions
    {
        public static string FormatLog(this IMessage message) =>
            $"Message {message.Id} in {message.Channel.FormatLog()}";
    }
}
