using Dapper;
using TaylorBot.Net.BirthdayReward.Domain;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.BirthdayReward.Infrastructure;

public class BirthdayCalendarPostgresRepository(PostgresConnectionFactory postgresConnectionFactory) : IBirthdayCalendarRepository
{
    public async Task RefreshBirthdayCalendarAsync()
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            "REFRESH MATERIALIZED VIEW CONCURRENTLY attributes.birthday_calendar_6months;",
            commandTimeout: (int)TimeSpan.FromMinutes(10).TotalSeconds);
    }
}
