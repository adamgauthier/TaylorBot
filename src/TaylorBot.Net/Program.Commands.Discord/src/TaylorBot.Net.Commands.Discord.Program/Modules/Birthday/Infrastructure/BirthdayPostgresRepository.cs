using Dapper;
using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Domain;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Infrastructure;

public class BirthdayPostgresRepository(PostgresConnectionFactory postgresConnectionFactory) : IBirthdayRepository
{
    private record BirthdayDto(DateOnly birthday, bool is_private);

    public async ValueTask<IBirthdayRepository.Birthday?> GetBirthdayAsync(IUser user)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        var birthday = await connection.QuerySingleOrDefaultAsync<BirthdayDto?>(
            """
            SELECT birthday, is_private
            FROM attributes.birthdays
            WHERE user_id = @UserId;
            """,
            new
            {
                UserId = $"{user.Id}",
            }
        );

        return birthday != null ? new(
            Date: birthday.birthday,
            IsPrivate: birthday.is_private
        ) : null;
    }

    public async ValueTask ClearBirthdayAsync(IUser user)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            "DELETE FROM attributes.birthdays WHERE user_id = @UserId;",
            new
            {
                UserId = $"{user.Id}",
            }
        );
    }

    public async ValueTask SetBirthdayAsync(IUser user, IBirthdayRepository.Birthday birthday)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            """
            INSERT INTO attributes.birthdays (user_id, birthday, is_private)
            VALUES (@UserId, @Birthday, @IsPrivate)
            ON CONFLICT (user_id) DO UPDATE SET
                birthday = excluded.birthday,
                is_private = excluded.is_private;
            """,
            new
            {
                UserId = $"{user.Id}",
                Birthday = birthday.Date,
                IsPrivate = birthday.IsPrivate,
            }
        );
    }

    private record CalendarEntryDto(string user_id, string username, DateOnly next_birthday);

    public async ValueTask<IList<IBirthdayRepository.BirthdayCalendarEntry>> GetBirthdayCalendarAsync(IGuild guild)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        var entries = await connection.QueryAsync<CalendarEntryDto>(
            """
            SELECT calendar.user_id, username, next_birthday
            FROM attributes.birthday_calendar_6months calendar
            JOIN guilds.guild_members AS gm ON calendar.user_id = gm.user_id AND gm.guild_id = @GuildId AND gm.alive = TRUE
            ORDER BY next_birthday
            LIMIT 150;
            """,
            new
            {
                GuildId = $"{guild.Id}",
            }
        );

        return entries.Select(e => new IBirthdayRepository.BirthdayCalendarEntry(
            new(e.user_id),
            e.username,
            e.next_birthday
        )).ToList();
    }

    private record AgeRoleDto(string age_role_id, int minimum_age);

    public async ValueTask<IList<IBirthdayRepository.AgeRole>> GetAgeRolesAsync(IGuild guild)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        var ageRoles = await connection.QueryAsync<AgeRoleDto>(
            """
            SELECT age_role_id, minimum_age
            FROM plus.age_roles
            INNER JOIN plus.plus_guilds
            ON plus.age_roles.guild_id = plus.plus_guilds.guild_id
            WHERE plus_guilds.guild_id = @GuildId
            AND plus_guilds.state = 'enabled';
            """,
            new
            {
                GuildId = $"{guild.Id}",
            }
        );

        return ageRoles.Select(a => new IBirthdayRepository.AgeRole(
            new(a.age_role_id),
            a.minimum_age
        )).ToList();
    }
}
