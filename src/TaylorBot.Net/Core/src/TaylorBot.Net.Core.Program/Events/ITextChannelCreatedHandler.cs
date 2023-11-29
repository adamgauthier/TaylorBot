using Discord.WebSocket;

namespace TaylorBot.Net.Core.Program.Events
{
    public interface ITextChannelCreatedHandler
    {
        Task TextChannelCreatedAsync(SocketTextChannel textChannel);
    }
}
