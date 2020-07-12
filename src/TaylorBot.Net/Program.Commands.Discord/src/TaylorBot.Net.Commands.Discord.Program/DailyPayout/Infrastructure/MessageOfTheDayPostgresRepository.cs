using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.DailyPayout.Domain;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.Commands.Discord.Program.DailyPayout.Infrastructure
{
    public class MessageOfTheDayPostgresRepository : IMessageOfTheDayRepository
    {
        private readonly PostgresConnectionFactory _postgresConnectionFactory;

        public MessageOfTheDayPostgresRepository(PostgresConnectionFactory postgresConnectionFactory)
        {
            _postgresConnectionFactory = postgresConnectionFactory;
        }

        private class MessageDto
        {
            public string message { get; set; } = null!;
            public DateTimeOffset? priority_from { get; set; }
            public DateTimeOffset? priority_to { get; set; }
        }

        public async ValueTask<IReadOnlyList<MessageOfTheDay>> GetAllMessagesAsync()
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            var messages = await connection.QueryAsync<MessageDto>(
                "SELECT message, priority_from, priority_to FROM commands.messages_of_the_day ORDER BY added_at ASC;"
            );

            return messages.Select(m => new MessageOfTheDay(
                message: m.message,
                messagePriority: m.priority_from.HasValue && m.priority_to.HasValue ?
                    new MessagePriority(m.priority_from.Value, m.priority_to.Value) :
                    null
            )).ToList();
        }
    }
}

