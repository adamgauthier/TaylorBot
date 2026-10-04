using Google.Apis.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TaylorBot.Net.IntegrationTests.Shared.ExternalApis;

namespace TaylorBot.Net.UserNotifier.IntegrationTests.Hosting;

internal static class NotifierTransports
{
    public static void Configure(IServiceCollection services, ExternalApi external)
    {
        services.ConfigureHttpClientDefaults(builder => builder.ConfigurePrimaryHttpMessageHandler(() => new Transport(external)));

        services.Replace(ServiceDescriptor.Singleton(new BaseClientService.Initializer
        {
            ApiKey = "synthetic",
            HttpClientFactory = new GoogleTransport(external),
        }));
    }

    private sealed class Transport(ExternalApi external) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            external.SendAsync(request, cancellationToken);
    }

    private sealed class GoogleTransport(ExternalApi external) : Google.Apis.Http.HttpClientFactory
    {
        protected override HttpMessageHandler CreateHandler(Google.Apis.Http.CreateHttpClientArgs args) => new Transport(external);
    }
}
