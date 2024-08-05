namespace TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Domain;

public interface IHoroscopeClient
{
    ValueTask<IHoroscopeResult> GetHoroscopeAsync(string zodiacSign);
}

public interface IHoroscopeResult { }
public record GaneshaSpeaksGenericErrorResult() : IHoroscopeResult;
public record Horoscope(string Text) : IHoroscopeResult;
