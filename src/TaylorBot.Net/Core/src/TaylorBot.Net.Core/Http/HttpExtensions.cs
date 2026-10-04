using Microsoft.Extensions.Logging;
using OperationResult;
using System.Net;
using System.Net.Http.Json;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Core.Http;

public record HttpError(Exception? Exception, string? Content, bool WasHttpSuccess, HttpStatusCode? StatusCode = null);

public record HttpSuccess<T>(T Parsed, HttpResponseMessage Response);

public static partial class HttpExtensions
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
                LogUnhandledErrorParsingSuccessContent(logger, stringException, jsonException.Message);
                return await handleErrorAsync(new HttpError(stringException, Content: null, WasHttpSuccess: true, response.StatusCode));
            }

            LogUnhandledErrorHandlingSuccessContent(logger, jsonException, content);
            return await handleErrorAsync(new HttpError(jsonException, content, WasHttpSuccess: true, response.StatusCode));
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
            LogUnhandledErrorParsingContent(logger, e);
            return Error(new HttpError(e, Content: null, WasHttpSuccess: true, response.StatusCode));
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
            LogUnhandledErrorMakingRequest(logger, e);
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
                LogErrorResponse(logger, response.StatusCode, content);
                return Error(new HttpError(null, content, WasHttpSuccess: false, response.StatusCode));
            }
            catch (Exception e)
            {
                LogUnhandledErrorReadingErrorContent(logger, e, response.StatusCode);
                return Error(new HttpError(e, Content: null, WasHttpSuccess: false, response.StatusCode));
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
            LogCannotReadContent(logger, e, message);
            return;
        }

        LogMessageWithContent(logger, level, message, content);
    }

    public static async Task EnsureSuccessAsync(this HttpResponseMessage response, ILogger logger)
    {
        var checkResult = await response.VerifyStatusAsync(logger);
        if (checkResult.IsError)
        {
            response.EnsureSuccessStatusCode();
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled error when parsing success content {JsonExceptionMessage}")]
    private static partial void LogUnhandledErrorParsingSuccessContent(ILogger logger, Exception exception, string jsonExceptionMessage);

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled error when handling success content {Content}")]
    private static partial void LogUnhandledErrorHandlingSuccessContent(ILogger logger, Exception exception, string content);

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled error when parsing success content")]
    private static partial void LogUnhandledErrorParsingContent(ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled error when making request")]
    private static partial void LogUnhandledErrorMakingRequest(ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Error response ({StatusCode}): {Content}")]
    private static partial void LogErrorResponse(ILogger logger, HttpStatusCode statusCode, string content);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Unhandled error when reading error content ({StatusCode})")]
    private static partial void LogUnhandledErrorReadingErrorContent(ILogger logger, Exception exception, HttpStatusCode statusCode);

    [LoggerMessage(Level = LogLevel.Error, Message = "{Message}: can't read content")]
    private static partial void LogCannotReadContent(ILogger logger, Exception exception, string message);

    [LoggerMessage(Message = "{Message}: {Content}")]
    private static partial void LogMessageWithContent(ILogger logger, LogLevel logLevel, string message, string content);
}
