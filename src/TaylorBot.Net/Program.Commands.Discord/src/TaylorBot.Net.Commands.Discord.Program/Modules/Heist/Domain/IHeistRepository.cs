using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Domain;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Heist.Domain;

public interface IEnterHeistResult { }
public record HeistCreated() : IEnterHeistResult;
public record HeistEntered() : IEnterHeistResult;
public record InvestmentUpdated() : IEnterHeistResult;

public record HeistPlayer(string UserId, ITaypointAmount Amount);

public interface IHeistRepository
{
    Task<IEnterHeistResult> EnterHeistAsync(IGuildUser user, ITaypointAmount amount, TimeSpan heistDelay);
    Task<List<HeistPlayer>> EndHeistAsync(IGuild guild);
}

