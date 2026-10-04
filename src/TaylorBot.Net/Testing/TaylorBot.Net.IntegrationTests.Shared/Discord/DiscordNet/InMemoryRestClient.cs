using System.Text;
using Discord.Net.Rest;

namespace TaylorBot.Net.IntegrationTests.Shared.Discord.DiscordNet;

public sealed class InMemoryRestClient(IDiscordApi api) : IRestClient
{
    private readonly Dictionary<string, string> _headers = [];
    private CancellationToken _cancelToken;

    public void SetHeader(string key, string value) => _headers[key] = value;
    public void SetCancelToken(CancellationToken cancelToken) => _cancelToken = cancelToken;
    public void Dispose() { }

    public Task<RestResponse> SendAsync(string method, string endpoint, CancellationToken cancelToken, bool headerOnly = false, string? reason = null,
        IEnumerable<KeyValuePair<string, IEnumerable<string>>>? requestHeaders = null)
    {
        _cancelToken.ThrowIfCancellationRequested();
        cancelToken.ThrowIfCancellationRequested();
        var response = api.Send(method, endpoint);

        return Task.FromResult(new RestResponse(response.Status, [], new MemoryStream(Encoding.UTF8.GetBytes(response.Body))));
    }

    public Task<RestResponse> SendAsync(string method, string endpoint, string json, CancellationToken cancelToken, bool headerOnly = false, string? reason = null,
        IEnumerable<KeyValuePair<string, IEnumerable<string>>>? requestHeaders = null)
    {
        _cancelToken.ThrowIfCancellationRequested();
        cancelToken.ThrowIfCancellationRequested();
        var response = api.Send(method, endpoint, json);

        return Task.FromResult(new RestResponse(response.Status, [], new MemoryStream(Encoding.UTF8.GetBytes(response.Body))));
    }

    public Task<RestResponse> SendAsync(string method, string endpoint, IReadOnlyDictionary<string, object> multipartParams, CancellationToken cancelToken,
        bool headerOnly = false, string? reason = null, IEnumerable<KeyValuePair<string, IEnumerable<string>>>? requestHeaders = null) =>
        throw api.UnexpectedRequest($"Unexpected Discord multipart request: {method} {endpoint}");
}
