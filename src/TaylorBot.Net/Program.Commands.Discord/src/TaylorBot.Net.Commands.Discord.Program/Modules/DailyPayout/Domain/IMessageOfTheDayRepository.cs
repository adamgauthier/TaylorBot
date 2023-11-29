namespace TaylorBot.Net.Commands.Discord.Program.Modules.DailyPayout.Domain
{
    public record MessagePriority(DateTimeOffset From, DateTimeOffset To);

    public record MessageOfTheDay(string Message, MessagePriority? MessagePriority);

    public interface IMessageOfTheDayRepository
    {
        ValueTask<IReadOnlyList<MessageOfTheDay>> GetAllMessagesAsync();
    }
}
