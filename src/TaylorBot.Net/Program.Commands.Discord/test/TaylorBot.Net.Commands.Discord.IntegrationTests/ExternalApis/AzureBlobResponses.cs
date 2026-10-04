using System.Net;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Scenarios;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.ExternalApis;

public static class AzureBlobResponses
{
    public static void SignatureExists(this ExternalApi api, ScenarioUser user, bool exists) =>
        api.Raw("HEAD", SignatureUri(user), "", "application/xml", exists ? HttpStatusCode.OK : HttpStatusCode.NotFound,
            new Dictionary<string, string> { ["x-ms-error-code"] = exists ? "" : "BlobNotFound" });

    public static void SignatureUpload(this ExternalApi api, ScenarioUser user) =>
        api.Raw("PUT", SignatureUri(user), "", "application/xml", HttpStatusCode.Created, new Dictionary<string, string>
        {
            ["ETag"] = "\"synthetic\"",
            ["Last-Modified"] = "Thu, 01 Jan 2026 00:00:00 GMT",
            ["x-ms-request-id"] = "synthetic",
            ["x-ms-request-server-encrypted"] = "true",
        });

    public static void SignatureList(this ExternalApi api, ScenarioUser user, bool exists)
    {
        var blob = exists
            ? $"""
              <Blob><Name>{user.Id}-{user.Username}.png</Name><Properties>
              <Creation-Time>Thu, 01 Jan 2026 00:00:00 GMT</Creation-Time>
              <Last-Modified>Thu, 01 Jan 2026 00:00:00 GMT</Last-Modified><Etag>"synthetic"</Etag>
              <Content-Length>4</Content-Length><Content-Type>image/png</Content-Type><BlobType>BlockBlob</BlobType>
              <LeaseStatus>unlocked</LeaseStatus><LeaseState>available</LeaseState><ServerEncrypted>true</ServerEncrypted>
              </Properties></Blob>
              """
            : "";
        api.Raw("GET", $"https://storage.invalid/signatures2025?restype=container&comp=list&prefix={user.Id}-&synthetic",
            $"""<?xml version="1.0" encoding="utf-8"?><EnumerationResults ServiceEndpoint="https://storage.invalid/" ContainerName="signatures2025"><Blobs>{blob}</Blobs><NextMarker /></EnumerationResults>""",
            "application/xml");
    }

    private static string SignatureUri(ScenarioUser user) =>
        $"https://storage.invalid/signatures2025/{user.Id}-{user.Username}.png?synthetic";
}
