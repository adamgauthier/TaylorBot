using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json;
using TaylorBot.Net.Commands.Discord.Program.Modules.WolframAlpha.Domain;
using TaylorBot.Net.Commands.Discord.Program.Options;
using static TaylorBot.Net.Commands.Discord.Program.Modules.WolframAlpha.Domain.WolframAlphaResult;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.WolframAlpha.Infrastructure;

public class WolframAlphaClient : IWolframAlphaClient
{
    private readonly ILogger<WolframAlphaClient> _logger;
    private readonly IOptionsMonitor<WolframAlphaOptions> _options;
    private readonly HttpClient _httpClient = new();

    public WolframAlphaClient(ILogger<WolframAlphaClient> logger, IOptionsMonitor<WolframAlphaOptions> options)
    {
        _logger = logger;
        _options = options;
    }

    public async ValueTask<IWolframAlphaResult> QueryAsync(string query)
    {
        var queryString = new[] {
            $"input={query}",
            $"appid={_options.CurrentValue.AppId}",
            $"output=json",
            $"ip=192.168.1.1",
            $"podindex=1,2"
        };
        try
        {
            var response = await _httpClient.GetAsync($"https://api.wolframalpha.com/v2/query?{string.Join('&', queryString)}");

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var jsonDocument = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
                var queryResult = jsonDocument.RootElement.GetProperty("queryresult");

                if (queryResult.TryGetProperty("success", out var isSuccess) && isSuccess.GetBoolean())
                {
                    var pods = queryResult.GetProperty("pods")
                        .EnumerateArray()
                        .Select(p => p.GetProperty("subpods").EnumerateArray().First())
                        .Select(p => new Pod(
                            p.GetProperty("plaintext").GetString()!,
                            p.GetProperty("img").GetProperty("src").GetString()!
                        ))
                        .ToList();

                    return new WolframAlphaResult(InputPod: pods[0], OutputPod: pods[1]);
                }
                else
                {
                    queryResult.TryGetProperty("error", out var error);
                    _logger.LogWarning("Error response from WolframAlpha: {Error}", error);
                    return new WolframAlphaFailed();
                }
            }
            else
            {
                try
                {
                    var body = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Error response from WolframAlpha ({StatusCode}): {Body}", response.StatusCode, body);
                    return new GenericWolframAlphaError();
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Unhandled error when parsing JSON in WolframAlpha error response ({StatusCode}):", response.StatusCode);
                    return new GenericWolframAlphaError();
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Unhandled error in WolframAlpha API:");
            return new GenericWolframAlphaError();
        }
    }
}
