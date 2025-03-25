using Dapper;
using TaylorBot.Net.Commands.Discord.Program.Modules.DailyPayout.Domain;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.DailyPayout.Infrastructure;

public class MessageOfTheDayPostgresRepository(PostgresConnectionFactory postgresConnectionFactory) : IMessageOfTheDayRepository
{
    private sealed class MessageDto
    {
        public string message { get; set; } = null!;
        public DateTimeOffset? priority_from { get; set; }
        public DateTimeOffset? priority_to { get; set; }
    }

    public async ValueTask<IReadOnlyList<MessageOfTheDay>> GetAllMessagesAsync()
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        var messages = await connection.QueryAsync<MessageDto>(
            "SELECT message, priority_from, priority_to FROM commands.messages_of_the_day ORDER BY added_at ASC;"
        );

        return [.. messages.Select(m => new MessageOfTheDay(
            Message: m.message,
            MessagePriority: m.priority_from.HasValue && m.priority_to.HasValue ?
                new MessagePriority(m.priority_from.Value, m.priority_to.Value) :
                null
        ))];
    }
}

