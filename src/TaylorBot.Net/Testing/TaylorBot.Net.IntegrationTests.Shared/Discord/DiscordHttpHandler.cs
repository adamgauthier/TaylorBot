using System.Text;
using TaylorBot.Net.IntegrationTests.Shared.ExternalApis;

namespace TaylorBot.Net.IntegrationTests.Shared.Discord;

public sealed class DiscordHttpHandler(IDiscordApi api, ExternalApi? external = null) : HttpMessageHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var uri = request.RequestUri ?? throw new InvalidOperationException("Missing HTTP request URI.");
        if (uri.Host != "discord.com" || !uri.AbsolutePath.StartsWith("/api/v10/", StringComparison.Ordinal))
        {
            if (external != null)
            {
                return await external.SendAsync(request, cancellationToken);
            }

            throw api.UnexpectedRequest($"Unconfigured external request: {request.Method} {uri}");
        }

        string? content = null;
        List<DiscordAttachment> attachments = [];
        if (request.Content is MultipartFormDataContent multipart)
        {
            foreach (var part in multipart)
            {
                if (part.Headers.ContentDisposition?.Name?.Trim('"') == "payload_json")
                {
                    content = await part.ReadAsStringAsync(cancellationToken);
                }
                else
                {
                    attachments.Add(new(part.Headers.ContentDisposition?.FileName?.Trim('"') ?? throw new InvalidOperationException("Missing attachment filename."),
                        await part.ReadAsByteArrayAsync(cancellationToken)));
                }
            }
        }
        else if (request.Content != null)
        {
            content = await request.Content.ReadAsStringAsync(cancellationToken);
        }

        var response = api.Send(request.Method.Method, uri.PathAndQuery["/api/v10/".Length..], content, attachments);
        return new(response.Status)
        {
            Content = new StringContent(response.Body, Encoding.UTF8, "application/json"),
        };
    }
}
