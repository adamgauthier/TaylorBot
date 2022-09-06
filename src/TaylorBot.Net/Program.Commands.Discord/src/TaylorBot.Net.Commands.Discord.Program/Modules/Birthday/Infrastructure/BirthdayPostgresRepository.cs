using Dapper;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Domain;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Infrastructure
{
    public class BirthdayPostgresRepository : IBirthdayRepository
    {
        private readonly PostgresConnectionFactory _postgresConnectionFactory;

        public BirthdayPostgresRepository(PostgresConnectionFactory postgresConnectionFactory)
        {
            _postgresConnectionFactory = postgresConnectionFactory;
        }

        private record BirthdayDto(DateOnly birthday, bool is_private);

        public async ValueTask<IBirthdayRepository.Birthday?> GetBirthdayAsync(IUser user)
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            var birthday = await connection.QuerySingleOrDefaultAsync<BirthdayDto?>(
                @"SELECT birthday, is_private
                FROM attributes.birthdays
                WHERE user_id = @UserId;",
                new
                {
                    UserId = user.Id.ToString(),
                }
            );

            return birthday != null ? new(
                Date: birthday.birthday,
                IsPrivate: birthday.is_private
            ) : null;
        }

        public async ValueTask ClearBirthdayAsync(IUser user)
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                @"DELETE FROM attributes.birthdays WHERE user_id = @UserId;",
                new
                {
                    UserId = user.Id.ToString(),
                }
            );
        }

        public async ValueTask SetBirthdayAsync(IUser user, IBirthdayRepository.Birthday birthday)
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                @"INSERT INTO attributes.birthdays (user_id, birthday, is_private)
                VALUES (@UserId, @Birthday, @IsPrivate)
                ON CONFLICT (user_id) DO UPDATE SET
                    birthday = excluded.birthday,
                    is_private = excluded.is_private;",
                new
                {
                    UserId = user.Id.ToString(),
                    Birthday = birthday.Date,
                    IsPrivate = birthday.IsPrivate,
                }
            );
        }

        public async ValueTask ClearLegacyAgeAsync(IUser user)
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                @"DELETE FROM attributes.integer_attributes
                WHERE attribute_id = 'age' AND user_id = @UserId;",
                new
                {
                    UserId = user.Id.ToString(),
                }
            );
        }

        private record CalendarEntryDto(string user_id, string username, DateOnly next_birthday);

        public async ValueTask<IList<IBirthdayRepository.BirthdayCalendarEntry>> GetBirthdayCalendarAsync(IGuild guild)
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            var entries = await connection.QueryAsync<CalendarEntryDto>(
                @"SELECT b.user_id, u.username,
                CASE
                    WHEN normalized_birthday < CURRENT_DATE
                    THEN (normalized_birthday + INTERVAL '1 YEAR')
                    ELSE normalized_birthday
                END
                AS next_birthday
                FROM
                attributes.birthdays b INNER JOIN users.users u ON b.user_id = u.user_id,
                make_date(
                    date_part('year', CURRENT_DATE)::int,
                    date_part('month', birthday)::int,
                    CASE
                        WHEN date_part('month', birthday)::int = 2 AND date_part('day', birthday)::int = 29
                        THEN 28
                        ELSE date_part('day', birthday)::int
                    END
                ) AS normalized_birthday
                WHERE is_private = FALSE
                AND b.user_id IN (
                   SELECT user_id
                   FROM guilds.guild_members
                   WHERE guild_id = @GuildId AND alive = TRUE
                )
                ORDER BY next_birthday
                LIMIT 100;",
                new
                {
                    GuildId = guild.Id.ToString(),
                }
            );

            return entries.Select(e => new IBirthdayRepository.BirthdayCalendarEntry(
                new(e.user_id),
                e.username,
                e.next_birthday
            )).ToList();
        }
    }
}
