using Discord;
using System.Threading.Tasks;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Domain
{
    public record LastFmUsername(string Username)
    {
        public string LinkToProfile => $"https://www.last.fm/user/{Username}/";
    }

    public interface ILastFmUsernameRepository
    {
        ValueTask<LastFmUsername?> GetLastFmUsernameAsync(IUser user);
        ValueTask SetLastFmUsernameAsync(IUser user, LastFmUsername lastFmUsername);
        ValueTask ClearLastFmUsernameAsync(IUser user);
    }
}
