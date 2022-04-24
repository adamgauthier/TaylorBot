using System;
using System.Threading.Tasks;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Knowledge.Domain
{
    public interface IHoroscopeClient
    {
        ValueTask<IHoroscopeResult> GetHoroscopeAsync(string zodiacSign);
    }

    public interface IHoroscopeResult { }
    public record GaneshaSpeaksGenericErrorResult(string? Error) : IHoroscopeResult;
    public record Horoscope(string Text, DateOnly Date) : IHoroscopeResult;
    public record HoroscopeUnavailable() : IHoroscopeResult;
}
