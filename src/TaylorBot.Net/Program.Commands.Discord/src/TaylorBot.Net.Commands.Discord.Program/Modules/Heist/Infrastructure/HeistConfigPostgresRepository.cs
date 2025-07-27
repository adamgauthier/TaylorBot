using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Heist.Infrastructure;

public sealed record Bank(string bankName, ushort? maximumUserCount, ushort minimumRollForSuccess, string payoutMultiplier);

public interface IHeistConfigRepository
{
    Task<List<Bank>> GetBanksAsync();
}

public class HeistConfigPostgresRepository(ILogger<HeistConfigPostgresRepository> logger, PostgresConnectionFactory postgresConnectionFactory, IMemoryCache memoryCache) : IHeistConfigRepository
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);
    private static readonly TimeSpan FailureCacheDuration = TimeSpan.FromMinutes(5);

    private static readonly List<Bank> DefaultBanks =
    [
        new("Beautiful Eyes Boutique Vault 🏚️", maximumUserCount: null, minimumRollForSuccess: 16, payoutMultiplier: "0.18"),
    ];

    public async Task<List<Bank>> GetBanksAsync()
    {
        var banks = await memoryCache.GetOrCreateAsync("heist_banks", async entry =>
        {
            try
            {
                var json = await FetchBanksJsonAsync();
                var banks = JsonSerializer.Deserialize<List<Bank>>(json);
                ArgumentNullException.ThrowIfNull(banks);
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;
                return banks;
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "Failed to fetch banks from DB, using default");
                entry.AbsoluteExpirationRelativeToNow = FailureCacheDuration;
                return DefaultBanks;
            }
        });
        ArgumentNullException.ThrowIfNull(banks);
        return banks;
    }

    private async Task<string> FetchBanksJsonAsync()
    {
        using var connection = postgresConnectionFactory.CreateConnection();
        return await connection.QuerySingleAsync<string>(
            "SELECT info_value FROM configuration.application_info WHERE info_key = 'banks_json';");
    }
}
