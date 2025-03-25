using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Commands.Discord.Program.Modules.WolframAlpha.Domain;
using TaylorBot.Net.Commands.Discord.Program.Options;
using TaylorBot.Net.Core.Http;
using static TaylorBot.Net.Commands.Discord.Program.Modules.WolframAlpha.Domain.WolframAlphaResult;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.WolframAlpha.Infrastructure;

public class WolframAlphaClient(ILogger<WolframAlphaClient> logger, IOptionsMonitor<WolframAlphaOptions> options, HttpClient client) : IWolframAlphaClient
{
    public async ValueTask<IWolframAlphaResult> QueryAsync(string query)
    {
        var queryString = new[] {
            $"input={query}",
            $"appid={options.CurrentValue.AppId}",
            $"output=json",
            $"ip=192.168.1.1",
            $"podindex=1,2"
        };

        return await client.ReadJsonWithErrorLogging<WolframResponse, IWolframAlphaResult>(
            c => c.GetAsync($"https://api.wolframalpha.com/v2/query?{string.Join('&', queryString)}"),
            handleSuccessAsync: HandleSuccessAsync,
            handleErrorAsync: error => Task.FromResult(HandleError(error)),
            logger);
    }

    private async Task<IWolframAlphaResult> HandleSuccessAsync(HttpSuccess<WolframResponse> result)
    {
        var queryResult = result.Parsed.queryresult;
        if (queryResult.success)
        {
            var pods = queryResult.pods
                .Select(p => p.subpods[0])
                .Select(p => new Pod(p.plaintext, p.img.src))
                .ToList();

            return new WolframAlphaResult(InputPod: pods[0], OutputPod: pods[1]);
        }
        else
        {
            await result.Response.LogContentAsync(logger, LogLevel.Warning, "Non-success queryResult");
            return new WolframAlphaFailed();
        }
    }

    private IWolframAlphaResult HandleError(HttpError error)
    {
        return new GenericWolframAlphaError();
    }

    private sealed record WolframResponse(WolframQueryResult queryresult);

    private sealed record WolframQueryResult(bool success, IReadOnlyList<WolframPod> pods);

    private sealed record WolframPod(IReadOnlyList<WolframSubPod> subpods);

    private sealed record WolframSubPod(string plaintext, WolframImg img);

    private sealed record WolframImg(string src);
}
