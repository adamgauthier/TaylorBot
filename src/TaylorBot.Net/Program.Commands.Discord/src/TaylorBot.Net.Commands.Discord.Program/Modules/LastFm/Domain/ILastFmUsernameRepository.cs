using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Domain;

public record LastFmUsername(string Username)
{
    public string LinkToProfile => $"https://www.last.fm/user/{Username}/";
}

public interface ILastFmUsernameRepository
{
    ValueTask<LastFmUsername?> GetLastFmUsernameAsync(DiscordUser user);
    ValueTask SetLastFmUsernameAsync(DiscordUser user, LastFmUsername lastFmUsername);
    ValueTask ClearLastFmUsernameAsync(DiscordUser user);
}
