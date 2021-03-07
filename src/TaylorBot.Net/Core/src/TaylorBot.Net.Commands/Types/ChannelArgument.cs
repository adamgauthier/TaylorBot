using Discord;

namespace TaylorBot.Net.Commands.Types
{
    public interface IChannelArgument<T> where T : class, IChannel
    {
        T Channel { get; }
    }

    public class ChannelArgument<T> : IChannelArgument<T>
        where T : class, IChannel
    {
        public T Channel { get; }

        public ChannelArgument(T channel)
        {
            Channel = channel;
        }
    }
}
