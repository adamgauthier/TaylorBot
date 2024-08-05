using Microsoft.Extensions.Logging;
using OperationResult;
using System.Net.Http.Json;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Core.Http;

public record HttpError(Exception? Exception, string? Content, bool WasHttpSuccess);

public record HttpSuccess<T>(T Parsed, HttpResponseMessage Response);

public static class HttpExtensions
{
    public static async Task<TResult> ReadJsonWithErrorLogging<TJson, TResult>(this HttpClient client,
        Func<HttpClient, Task<HttpResponseMessage>> makeRequestAsync,
        Func<HttpSuccess<TJson>, Task<TResult>> handleSuccessAsync,
        Func<HttpError, Task<TResult>> handleErrorAsync,
        ILogger logger)
    {
        var requestResult = await client.MakeRequestAsync(makeRequestAsync, logger);
        if (requestResult.IsError)
        {
            return await handleErrorAsync(requestResult.Error);
        }

        using var response = requestResult.Value;

        var checkResult = await response.VerifyStatusAsync(logger);
        if (checkResult.IsError)
        {
            return await handleErrorAsync(checkResult.Error);
        }

        TJson? json;
        try
        {
            json = await response.Content.ReadFromJsonAsync<TJson>();
            ArgumentNullException.ThrowIfNull(json);
        }
        catch (Exception jsonException)
        {
            string content;
            try
            {
                content = await response.Content.ReadAsStringAsync();
            }
            catch (Exception stringException)
            {
                logger.LogError(stringException, "Unhandled error when parsing success content {JsonExceptionMessage}", jsonException.Message);
                return await handleErrorAsync(new HttpError(stringException, Content: null, WasHttpSuccess: true));
            }

            logger.LogError(jsonException, "Unhandled error when handling success content {Content}", content);
            return await handleErrorAsync(new HttpError(jsonException, content, WasHttpSuccess: true));
        }

        try
        {
            return await handleSuccessAsync(new(json, response));
        }
        catch
        {
            await response.LogContentAsync(logger, LogLevel.Error, "Success handler failed");
            throw;
        }
    }

    public static async Task<Result<string, HttpError>> ReadStringWithErrorLogging(this HttpClient client, Func<HttpClient, Task<HttpResponseMessage>> makeRequestAsync, ILogger logger)
    {
        var requestResult = await client.MakeRequestAsync(makeRequestAsync, logger);
        if (requestResult.IsError)
        {
            return Error(requestResult.Error);
        }

        using var response = requestResult.Value;

        var checkResult = await response.VerifyStatusAsync(logger);
        if (checkResult.IsError)
        {
            return Error(checkResult.Error);
        }

        try
        {
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unhandled error when parsing success content");
            return Error(new HttpError(e, Content: null, WasHttpSuccess: true));
        }
    }

    public static async Task<Result<HttpResponseMessage, HttpError>> MakeRequestAsync(this HttpClient client, Func<HttpClient, Task<HttpResponseMessage>> makeRequestAsync, ILogger logger)
    {
        try
        {
            return await makeRequestAsync(client);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unhandled error when making request");
            return Error(new HttpError(e, Content: null, WasHttpSuccess: false));
        }
    }

    public static async Task<Result<object?, HttpError>> VerifyStatusAsync(this HttpResponseMessage response, ILogger logger)
    {
        if (!response.IsSuccessStatusCode)
        {
            try
            {
                var content = await response.Content.ReadAsStringAsync();
                logger.LogWarning("Error response ({StatusCode}): {Content}", response.StatusCode, content);
                return Error(new HttpError(null, content, WasHttpSuccess: false));
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "Unhandled error when reading error content ({StatusCode})", response.StatusCode);
                return Error(new HttpError(e, Content: null, WasHttpSuccess: false));
            }
        }

        return null;
    }

    public static async Task LogContentAsync(this HttpResponseMessage response,
        ILogger logger,
        LogLevel level,
        string message)
    {
        string content;
        try
        {
            content = await response.Content.ReadAsStringAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "{Message}: can't read content", message);
            return;
        }

        logger.Log(level, "{Message}: {Content}", message, content);
    }

    public static async Task EnsureSuccessAsync(this HttpResponseMessage response, ILogger logger)
    {
        var checkResult = await response.VerifyStatusAsync(logger);
        if (checkResult.IsError)
        {
            response.EnsureSuccessStatusCode();
        }
    }
}
