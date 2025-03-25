using Microsoft.Extensions.Logging;
using System.Text.Json;
using TaylorBot.Net.Commands.Discord.Program.Modules.Imgur.Domain;
using TaylorBot.Net.Core.Http;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Imgur.Infrastructure;

public class ImgurHttpClient(ILogger<ImgurHttpClient> logger, HttpClient client) : IImgurClient
{
    public async ValueTask<IImgurResult> UploadAsync(string url)
    {
        using FormUrlEncodedContent content = new(new Dictionary<string, string>
        {
            { "image", url },
        });

        return await client.ReadJsonWithErrorLogging<ImgurSuccessResponse, IImgurResult>(
            c => c.PostAsync("https://api.imgur.com/3/image", content),
            handleSuccessAsync: HandleSuccessAsync,
            handleErrorAsync: error => Task.FromResult(HandleError(error)),
            logger);
    }

    private async Task<IImgurResult> HandleSuccessAsync(HttpSuccess<ImgurSuccessResponse> result)
    {
        if (result.Parsed.success)
        {
            return new UploadSuccess(Url: result.Parsed.data.link);
        }
        else
        {
            await result.Response.LogContentAsync(logger, LogLevel.Warning, "Non-success response");
            return new GenericImgurError();
        }
    }

    private IImgurResult HandleError(HttpError error)
    {
        if (error.Content != null)
        {
            var response = JsonSerializer.Deserialize<ImgurErrorResponse>(error.Content);
            var errorMessage = response?.data.error;

            if (errorMessage?.Equals("we don't support that file type!", StringComparison.OrdinalIgnoreCase) == true)
            {
                return new FileTypeInvalid();
            }
            else if (errorMessage?.Equals("file is over the size limit", StringComparison.OrdinalIgnoreCase) == true)
            {
                return new FileTooLarge();
            }
            else
            {
                return new GenericImgurError();
            }
        }
        else
        {
            return new GenericImgurError();
        }
    }

    private sealed record ImgurSuccessResponse(bool success, ImgurSuccessResponse.ImgurSuccessData data)
    {
        public sealed record ImgurSuccessData(string link);
    }

    private sealed record ImgurErrorResponse(ImgurErrorResponse.ImgurErrorData data)
    {
        public sealed record ImgurErrorData(string error);
    }
}
