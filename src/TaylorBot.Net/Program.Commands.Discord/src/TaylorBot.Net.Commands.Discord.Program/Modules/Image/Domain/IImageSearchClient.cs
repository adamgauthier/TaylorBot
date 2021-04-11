using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Image.Domain
{
    public interface IImageSearchResult { }

    public record ImageResult(string Title, string PageUrl, string ImageUrl);
    public record SuccessfulSearch(IReadOnlyList<ImageResult> Images, string ResultCount, string SearchTimeSeconds) : IImageSearchResult;

    public record DailyLimitExceeded() : IImageSearchResult;
    public record GenericError(Exception Exception) : IImageSearchResult;

    public interface IImageSearchClient
    {
        ValueTask<IImageSearchResult> SearchImagesAsync(string query);
    }
}
