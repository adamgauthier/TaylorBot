using System.Net;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Scenarios;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.ExternalApis;

public static class OwnerResponses
{
    public static byte[] AvatarTransfer(this ExternalApi api, ScenarioUser user)
    {
        var image = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+aP1sAAAAASUVORK5CYII=");
        api.Bytes($"https://cdn.discordapp.com/avatars/{user.Id}/{user.Avatar}.png?size=2048", image);
        api.Raw("PUT", $"https://storage.invalid/avatars2025/{user.Id}-{user.Username}.png?synthetic",
            "", "application/xml", HttpStatusCode.Created, new Dictionary<string, string>
            {
                ["ETag"] = "\"synthetic-avatar\"",
                ["Last-Modified"] = "Thu, 01 Jan 2026 00:00:00 GMT",
                ["x-ms-request-id"] = "synthetic-avatar",
                ["x-ms-request-server-encrypted"] = "true",
            });
        return image;
    }
}
