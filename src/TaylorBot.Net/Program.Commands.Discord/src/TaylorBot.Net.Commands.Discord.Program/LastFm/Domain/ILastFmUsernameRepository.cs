using Discord;
using System.Threading.Tasks;

namespace TaylorBot.Net.Commands.Discord.Program.LastFm.Domain
{
    public class LastFmUsername
    {
        public string Username { get; }

        public string LinkToProfile => $"https://www.last.fm/user/{Username}/";

        public LastFmUsername(string username)
        {
            Username = username;
        }
    }

    public interface ILastFmUsernameRepository
    {
        ValueTask<LastFmUsername?> GetLastFmUsernameAsync(IUser user);
        ValueTask SetLastFmUsernameAsync(IUser user, LastFmUsername lastFmUsername);
    }
}
