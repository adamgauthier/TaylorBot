using Microsoft.Extensions.Logging;
using System.Text.Json;
using TaylorBot.Net.Commands.Discord.Program.Modules.Imgur.Domain;
using TaylorBot.Net.Core.Http;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Imgur.Commands;

public class ImgurHttpClient(ILogger<ImgurHttpClient> logger, HttpClient client) : IImgurClient
{
    public async ValueTask<IImgurResult> UploadAsync(string url)
    {
        FormUrlEncodedContent content = new(new Dictionary<string, string>
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

            return response?.data.error.ToLowerInvariant() switch
            {
                "we don't support that file type!" => new FileTypeInvalid(),
                "file is over the size limit" => new FileTooLarge(),
                _ => new GenericImgurError(),
            };
        }
        else
        {
            return new GenericImgurError();
        }
    }

    private record ImgurSuccessResponse(bool success, ImgurSuccessResponse.ImgurSuccessData data)
    {
        public record ImgurSuccessData(string link);
    }

    private record ImgurErrorResponse(ImgurErrorResponse.ImgurErrorData data)
    {
        public record ImgurErrorData(string error);
    }
}
