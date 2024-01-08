using Discord;

namespace TaylorBot.Net.Commands.Types;

public interface IChannelArgument<T> where T : class, IChannel
{
    T Channel { get; }
}

public class ChannelArgument<T>(T channel) : IChannelArgument<T>
    where T : class, IChannel
{
    public T Channel { get; } = channel;
}
