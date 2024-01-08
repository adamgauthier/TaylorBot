using Dapper;
using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Stats.Domain;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Stats.Infrastructure;

public class ServerStatsRepositoryPostgresRepository(PostgresConnectionFactory postgresConnectionFactory) : IServerStatsRepository
{
    private class AgeStatsDto
    {
        public decimal? age_average { get; set; }
        public decimal? age_median { get; set; }
    }

    public async ValueTask<AgeStats> GetAgeStatsInGuildAsync(IGuild guild)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        var ageStats = await connection.QuerySingleAsync<AgeStatsDto>(
            @"SELECT ROUND(AVG(human_age), 2) AS age_average, ROUND(MEDIAN(human_age), 2) AS age_median
                FROM (
                    SELECT date_part('year', age(birthday))::int AS human_age
                    FROM attributes.birthdays
                    WHERE date_part('year', birthday)::int != 1804 AND user_id IN (
                        SELECT user_id
                        FROM guilds.guild_members
                        WHERE guild_id = @GuildId AND alive = TRUE
                    )
                ) AS ages;",
            new
            {
                GuildId = guild.Id.ToString()
            }
        );

        return new AgeStats(ageStats.age_average, ageStats.age_median);
    }

    private class GenderStatsDto
    {
        public long total_count { get; set; }
        public long female_count { get; set; }
        public long male_count { get; set; }
        public long other_count { get; set; }
    }

    public async ValueTask<GenderStats> GetGenderStatsInGuildAsync(IGuild guild)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        var genderStats = await connection.QuerySingleAsync<GenderStatsDto>(
            @"SELECT
                    COUNT(*) AS total_count,
                    SUM(CASE WHEN attribute_value = 'Male' THEN 1 ELSE 0 END) AS male_count,
                    SUM(CASE WHEN attribute_value = 'Female' THEN 1 ELSE 0 END) AS female_count,
                    SUM(CASE WHEN attribute_value = 'Other' THEN 1 ELSE 0 END) AS other_count
                FROM attributes.text_attributes
                WHERE user_id IN (
                    SELECT user_id
                    FROM guilds.guild_members
                    WHERE guild_id = @GuildId
                )
                AND attribute_id = 'gender';",
            new
            {
                GuildId = guild.Id.ToString()
            }
        );

        return new GenderStats(genderStats.total_count, genderStats.male_count, genderStats.female_count, genderStats.other_count);
    }
}
