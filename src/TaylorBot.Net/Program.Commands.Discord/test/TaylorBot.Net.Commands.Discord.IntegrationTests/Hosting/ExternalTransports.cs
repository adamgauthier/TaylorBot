using Azure;
using Azure.Core.Pipeline;
using Azure.Storage.Blobs;
using Google.Apis.Services;
using GoogleHttpClientFactory = Google.Apis.Http.HttpClientFactory;
using CreateHttpClientArgs = Google.Apis.Http.CreateHttpClientArgs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Discord;
using TaylorBot.Net.Commands.Discord.IntegrationTests.ExternalApis;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;

internal static class ExternalTransports
{
    public static void Configure(IServiceCollection services, DiscordApi api, ExternalApi external)
    {
        services.ConfigureHttpClientDefaults(builder => builder.ConfigurePrimaryHttpMessageHandler(() => new DiscordHttpHandler(api, external)));

        services.Replace(ServiceDescriptor.Singleton(new BaseClientService.Initializer
        {
            ApiKey = "synthetic",
            HttpClientFactory = new GoogleTransport(api, external),
        }));

        services.AddKeyedSingleton("BlobTransport", (provider, _) =>
            provider.GetRequiredService<IHttpClientFactory>().CreateClient("BlobTransport"));
        services.RemoveAllKeyed<BlobServiceClient>("SignatureAccount");
        services.AddKeyedSingleton<BlobServiceClient>("SignatureAccount", (provider, _) =>
            new(new Uri("https://storage.invalid"), new AzureSasCredential("synthetic"), new BlobClientOptions
            {
                Transport = new HttpClientTransport(provider.GetRequiredKeyedService<HttpClient>("BlobTransport")),
            }));
    }

    private sealed class GoogleTransport(DiscordApi api, ExternalApi external) : GoogleHttpClientFactory
    {
        protected override HttpMessageHandler CreateHandler(CreateHttpClientArgs args) => new DiscordHttpHandler(api, external);
    }
}
