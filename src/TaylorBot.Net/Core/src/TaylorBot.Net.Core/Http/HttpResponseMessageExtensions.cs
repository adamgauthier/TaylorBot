using Microsoft.Extensions.Logging;

namespace TaylorBot.Net.Core.Http;

public static class HttpResponseMessageExtensions
{
    public static async ValueTask EnsureSuccessAsync(this HttpResponseMessage response, ILogger logger)
    {
        if (!response.IsSuccessStatusCode)
        {
            try
            {
                var body = await response.Content.ReadAsStringAsync();
                logger.LogError("Error response ({StatusCode}): {Body}", response.StatusCode, body);
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "Unhandled error when parsing error body ({StatusCode}):", response.StatusCode);
            }

            response.EnsureSuccessStatusCode();
        }
    }
}
