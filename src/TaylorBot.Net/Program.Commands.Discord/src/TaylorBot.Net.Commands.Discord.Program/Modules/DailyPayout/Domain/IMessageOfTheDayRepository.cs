namespace TaylorBot.Net.Commands.Discord.Program.Modules.DailyPayout.Domain;

public record MessagePriority(DateTimeOffset From, DateTimeOffset To);

public record MessageOfTheDay(Guid Id, string Message, MessagePriority? MessagePriority = null);

public interface IMessageOfTheDayRepository
{
    ValueTask<IReadOnlyList<MessageOfTheDay>> GetAllMessagesAsync();
}
