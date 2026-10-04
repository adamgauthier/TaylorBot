using System.Text.Json;
using TaylorBot.Net.IntegrationTests.Shared.ExternalApis;

namespace TaylorBot.Net.UserNotifier.IntegrationTests.ExternalApis;

public sealed class PatreonFixturesTests
{
    [Fact]
    public async Task LastPage_OmitsPaginationLinks()
    {
        ExternalApi external = new();
        PatreonFixtures fixtures = new(external);
        fixtures.Members([]);

        using HttpRequestMessage request = new(HttpMethod.Get, PatreonFixtures.MembersUri);
        using var response = await external.SendAsync(request, TestContext.Current.CancellationToken);
        using var page = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

        page.RootElement.TryGetProperty("links", out _).Should().BeFalse();
        external.EnsureExpectationsMet();
    }
}
