namespace TaylorBot.Net.Commands.Discord.Program.Modules.YouTube.Domain;

public interface IYouTubeSearchResult { }
public record SuccessfulSearch(IReadOnlyList<string> VideoUrls) : IYouTubeSearchResult;
public record GenericError(Exception Exception) : IYouTubeSearchResult;

public interface IYouTubeClient
{
    ValueTask<IYouTubeSearchResult> SearchAsync(string query);
}
