using Dapper;
using Discord;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.Reminders.Domain;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Reminders.Infrastructure
{
    public class ReminderPostgresRepository : IReminderRepository
    {
        private readonly PostgresConnectionFactory _postgresConnectionFactory;

        public ReminderPostgresRepository(PostgresConnectionFactory postgresConnectionFactory)
        {
            _postgresConnectionFactory = postgresConnectionFactory;
        }

        public async ValueTask<long> GetReminderCountAsync(IUser user)
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            return await connection.QuerySingleAsync<int>(
                @"SELECT COUNT(*) FROM users.reminders
                WHERE user_id = @UserId;",
                new
                {
                    UserId = user.Id.ToString()
                }
            );
        }

        public async ValueTask AddReminderAsync(IUser user, DateTimeOffset remindAt, string text)
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                @"INSERT INTO users.reminders (user_id, remind_at, reminder_text)
                VALUES (@UserId, @RemindAt, @ReminderText);",
                new
                {
                    UserId = user.Id.ToString(),
                    RemindAt = remindAt,
                    ReminderText = text,
                }
            );
        }
    }
}
