using TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Domain;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Heist.Domain;

public interface IEnterHeistResult { }
public record HeistCreated() : IEnterHeistResult;
public record HeistEntered() : IEnterHeistResult;
public record InvestmentUpdated() : IEnterHeistResult;

public record HeistPlayer(string UserId, ITaypointAmount Amount);

public interface IHeistRepository
{
    Task<IEnterHeistResult> EnterHeistAsync(DiscordMember member, ITaypointAmount amount, TimeSpan heistDelay);
    Task<List<HeistPlayer>> EndHeistAsync(CommandGuild guild);
}

