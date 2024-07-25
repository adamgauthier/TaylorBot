using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TaylorBot.Net.Core.Http;

namespace TaylorBot.Net.RedditNotifier.Domain;

public class RedditTokenInMemoryRepository(IServiceProvider serviceProvider, ILogger<RedditTokenInMemoryRepository> logger)
{
    private RedditToken? _token;

    public async Task<string> GetValidTokenAsync()
    {
        if (_token?.ExpiresAt >= DateTimeOffset.UtcNow + TimeSpan.FromSeconds(5))
        {
            return _token.AccessToken;
        }
        logger.LogInformation("Current token expires at {TokenExpiresAt}, requesting new token", _token?.ExpiresAt);

        var client = serviceProvider.GetRequiredService<RedditAuthHttpClient>();

        var token = await client.GetTokenAsync();
        _token = token;
        return token.AccessToken;
    }
}

public record RedditToken(string AccessToken, DateTimeOffset ExpiresAt);

public class RedditAuthHttpClient(ILogger<RedditAuthHttpClient> logger, HttpClient httpClient)
{
    private record RedditTokenResponse(string access_token, int expires_in);

    public async Task<RedditToken> GetTokenAsync()
    {
        var now = DateTimeOffset.UtcNow;

        var response = await httpClient.PostAsync("https://www.reddit.com/api/v1/access_token", new FormUrlEncodedContent([
            new("grant_type", "client_credentials"),
            new("scope", "read"),
        ]));
        await response.EnsureSuccessAsync(logger);

        var responseAsString = await response.Content.ReadAsStringAsync();

        var token = JsonSerializer.Deserialize<RedditTokenResponse>(responseAsString);
        ArgumentNullException.ThrowIfNull(token);

        return new(token.access_token, now + TimeSpan.FromSeconds(token.expires_in));
    }
}
