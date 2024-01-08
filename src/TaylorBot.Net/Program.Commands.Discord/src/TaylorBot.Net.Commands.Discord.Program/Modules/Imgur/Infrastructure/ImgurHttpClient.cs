using Microsoft.Extensions.Logging;
using System.Text.Json;
using TaylorBot.Net.Commands.Discord.Program.Modules.Imgur.Domain;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Imgur.Commands;

public class ImgurHttpClient(ILogger<ImgurHttpClient> logger, HttpClient httpClient) : ImgurClient
{
    public async ValueTask<IImgurResult> UploadAsync(string url)
    {
        try
        {
            FormUrlEncodedContent content = new(new Dictionary<string, string>
            {
                { "image", url },
            });

            var response = await httpClient.PostAsync("https://api.imgur.com/3/image", content);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStreamAsync();
                var jsonDocument = await JsonDocument.ParseAsync(responseString);

                if (jsonDocument.RootElement.TryGetProperty("success", out var isSuccess) && isSuccess.GetBoolean())
                {
                    var link = jsonDocument.RootElement.GetProperty("data").GetProperty("link").GetString();

                    if (link == null)
                    {
                        logger.LogWarning("Error response from Imgur: {Response}", responseString);
                        throw new InvalidOperationException("link property is null");
                    }

                    return new UploadSuccess(Url: link);
                }
                else
                {
                    logger.LogWarning("Error response from Imgur: {Response}", responseString);
                    return new GenericImgurError();
                }
            }
            else
            {
                try
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    logger.LogWarning("Error response from Imgur ({StatusCode}): {Response}", response.StatusCode, responseString);

                    var jsonDocument = JsonDocument.Parse(responseString);
                    var errorCode = jsonDocument.RootElement.GetProperty("data").GetProperty("error").GetProperty("code").GetInt32();

                    switch (errorCode)
                    {
                        case 1003:
                            return new FileTypeInvalid();

                        default:
                            return new GenericImgurError();
                    }
                }
                catch (Exception e)
                {
                    logger.LogWarning(e, "Unhandled error when parsing JSON in Imgur error response ({StatusCode}):", response.StatusCode);
                    return new GenericImgurError();
                }
            }
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Unhandled error in Imgur API:");
            return new GenericImgurError();
        }
    }
}
