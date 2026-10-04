using System.Net;
using System.Text.Json;

namespace TaylorBot.Net.IntegrationTests.Shared.Discord;

public sealed record DiscordAttachment(string Name, IReadOnlyList<byte> Bytes);
public sealed record DiscordRequest(string Method, string Path, JsonElement? Body, IReadOnlyList<DiscordAttachment>? Attachments = null);

public interface IDiscordApi
{
    (HttpStatusCode Status, string Body) Send(string method, string endpoint, string? json = null, IReadOnlyList<DiscordAttachment>? attachments = null);
    InvalidOperationException UnexpectedRequest(string message);
}
