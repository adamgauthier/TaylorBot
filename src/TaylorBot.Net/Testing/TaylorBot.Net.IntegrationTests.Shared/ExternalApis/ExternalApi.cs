using System.Net;
using System.Text;
using System.Text.Json;

namespace TaylorBot.Net.IntegrationTests.Shared.ExternalApis;

public sealed record ExternalRequest(string Method, Uri Uri, string? Body, IReadOnlyList<byte>? Bytes = null)
{
    public IReadOnlyDictionary<string, string> Headers { get; init; } = new Dictionary<string, string>();
}

public sealed class ExternalApi
{
    private sealed record Route(string Method, Uri Uri, byte[] Body, string ContentType, HttpStatusCode Status, string? NumericQueryParameter, IReadOnlyDictionary<string, string>? Headers)
    {
        public int Calls { get; set; }
        public ResponseGate? Gate { get; set; }
    }

    private readonly Lock _lock = new();
    private readonly List<Route> _routes = [];
    private readonly List<ExternalRequest> _requests = [];
    private readonly List<string> _failures = [];

    public IReadOnlyList<ExternalRequest> Requests { get { lock (_lock) { return [.. _requests]; } } }

    public void Json(string method, string uri, object body, HttpStatusCode status = HttpStatusCode.OK, string? numericQueryParameter = null) =>
        Add(method, uri, JsonSerializer.SerializeToUtf8Bytes(body), "application/json", status, numericQueryParameter);

    public void Bytes(string uri, byte[] body, string contentType = "image/png") =>
        Add("GET", uri, body, contentType, HttpStatusCode.OK, null);

    public void Raw(string method, string uri, string body, string contentType, HttpStatusCode status = HttpStatusCode.OK,
        IReadOnlyDictionary<string, string>? headers = null) =>
        Add(method, uri, Encoding.UTF8.GetBytes(body), contentType, status, null, headers);

    private void Add(string method, string uri, byte[] body, string contentType, HttpStatusCode status, string? numericQueryParameter,
        IReadOnlyDictionary<string, string>? headers = null)
    {
        lock (_lock) { _routes.Add(new(method, new(uri), body, contentType, status, numericQueryParameter, headers)); }
    }

    public ResponseGate Pause(string uri)
    {
        lock (_lock)
        {
            var route = _routes.Single(route => Canonical(route.Uri, route.NumericQueryParameter) == Canonical(new(uri), route.NumericQueryParameter));
            return route.Gate = new();
        }
    }

    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var bytes = request.Content == null ? null : await request.Content.ReadAsByteArrayAsync(cancellationToken);
        var body = request.Content == null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
        var uri = request.RequestUri ?? throw new InvalidOperationException("External request must have a URI.");

        Route route;
        lock (_lock)
        {
            _requests.Add(new(request.Method.Method, uri, body, bytes)
            {
                Headers = request.Headers.ToDictionary(header => header.Key, header => string.Join(", ", header.Value), StringComparer.OrdinalIgnoreCase),
            });

            var matched = _routes.SingleOrDefault(route => route.Method == request.Method.Method &&
                Canonical(route.Uri, route.NumericQueryParameter) == Canonical(uri, route.NumericQueryParameter));
            if (matched == null)
            {
                var failure = $"Unexpected external request: {request.Method} {uri}\n{body}";
                _failures.Add(failure);
                throw new InvalidOperationException(failure);
            }

            route = matched;
            route.Calls++;
        }

        if (route.Gate != null)
        {
            await route.Gate.WaitForReleaseAsync(cancellationToken);
        }

        HttpResponseMessage response = new(route.Status)
        {
            Content = new ByteArrayContent(route.Body) { Headers = { ContentType = new(route.ContentType) } },
        };
        if (route.Headers != null)
        {
            foreach (var header in route.Headers)
            {
                if (!response.Headers.TryAddWithoutValidation(header.Key, header.Value))
                {
                    response.Content.Headers.Add(header.Key, header.Value);
                }
            }
        }

        return response;
    }

    public sealed class ResponseGate : IDisposable
    {
        private readonly TaskCompletionSource _requested = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource _release = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task WaitForRequestAsync(CancellationToken cancellationToken) => _requested.Task.WaitAsync(cancellationToken);

        internal Task WaitForReleaseAsync(CancellationToken cancellationToken)
        {
            _requested.TrySetResult();
            return _release.Task.WaitAsync(cancellationToken);
        }

        public void Dispose() => _release.TrySetResult();
    }

    private static string Canonical(Uri uri, string? numericQueryParameter) =>
        uri.GetLeftPart(UriPartial.Path) + "?" + string.Join('&',
            uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries)
                .Select(value => Uri.UnescapeDataString(value.Replace("+", " ", StringComparison.Ordinal)))
                .Select(value => numericQueryParameter != null && value.StartsWith($"{numericQueryParameter}=", StringComparison.Ordinal)
                    && ulong.TryParse(value[(numericQueryParameter.Length + 1)..], out _) ? $"{numericQueryParameter}=<number>" : value)
                .Order(StringComparer.Ordinal));

    public void EnsureNoUnexpectedRequests()
    {
        lock (_lock)
        {
            if (_failures.Count != 0)
            {
                throw new InvalidOperationException(string.Join(Environment.NewLine, _failures));
            }
        }
    }

    public void EnsureExpectationsMet()
    {
        EnsureNoUnexpectedRequests();

        lock (_lock)
        {
            var unused = _routes.Where(route => route.Calls == 0).Select(route => $"{route.Method} {route.Uri}").ToArray();
            if (unused.Length != 0)
            {
                throw new InvalidOperationException($"Expected external requests were not made:\n{string.Join('\n', unused)}");
            }
        }
    }
}
